using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using FlowManager.Shared.DTOs;
using Microsoft.Extensions.Configuration;
using FlowManager.Client.DTOs.Chat;

namespace FlowManager.Client.Services
{
    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _sessionId;
        private readonly string _AiApiKey;
        private readonly bool _useAI;
        private readonly List<ChatMessage> _conversationHistory;
        private readonly Dictionary<string, Func<Dictionary<string, object>, Task<string>>> _availableActions;
        private IJSRuntime? _jsRuntime;

        public ChatService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _sessionId = Guid.NewGuid().ToString();
            _conversationHistory = new List<ChatMessage>();

            _AiApiKey = _configuration["GoogleAI:ApiKey"] ?? "";
            _useAI = !string.IsNullOrEmpty(_AiApiKey) && _AiApiKey != "your-google-ai-api-key-here";

            _availableActions = InitializeAgentActions();

            Console.WriteLine($"[ChatService] Google AI ApiKey found: {(!string.IsNullOrEmpty(_AiApiKey) ? "YES" : "NO")}");
            Console.WriteLine($"[ChatService] ApiKey length: {_AiApiKey.Length}");
            Console.WriteLine($"[ChatService] ApiKey starts with: {(_AiApiKey.Length > 10 ? _AiApiKey.Substring(0, 10) + "..." : _AiApiKey)}");
            Console.WriteLine($"[ChatService] _useAI: {_useAI}");
            Console.WriteLine($"[ChatService] AI Agent Mode: {(_useAI ? "Enabled with " + _availableActions.Count + " actions (Google Gemini)" : "Disabled")}");
        }

        public void SetJSRuntime(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task InitializeAsync()
        {
            await Task.CompletedTask;
        }

        public async Task<ChatResponse?> SendMessageAsync(string message)
        {
            try
            {
                Console.WriteLine($"[ChatService] Agent processing message: {message}");

                if (!_useAI)
                {
                    return new ChatResponse
                    {
                        Content = "AI Agent is not configured. Please set up OpenAI API key to use agent capabilities.",
                        IsMarkdown = false,
                        SessionId = _sessionId,
                        IsSuccessful = false
                    };
                }

                _conversationHistory.Add(new ChatMessage
                {
                    Content = message,
                    IsFromUser = true,
                    Timestamp = DateTime.Now
                });

                var response = await GenerateAgentResponse(message);

                if (response != null && response.IsSuccessful)
                {
                    _conversationHistory.Add(new ChatMessage
                    {
                        Content = response.Content,
                        IsFromUser = false,
                        Timestamp = DateTime.Now
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatService] Error in agent processing: {ex.Message}");

                return new ChatResponse
                {
                    Content = $"I encountered an error while processing your request: {ex.Message}",
                    IsMarkdown = false,
                    SessionId = _sessionId,
                    IsSuccessful = false
                };
            }
        }

        private async Task<ChatResponse> GenerateAgentResponse(string message)
        {
            try
            {
                Console.WriteLine($"[ChatService] AI Agent processing: {message}");

                const string systemPrompt = @"You are an AI assistant for FlowManager, a Blazor workflow management application.

## AVAILABLE ACTIONS:
1. **navigate_to_page** - Navigate to different pages
2. **fill_form_field** - Fill form fields with data
3. **click_button** - Click buttons and interactive elements
4. **submit_form** - Submit completed forms
5. **get_page_info** - Get information about current page
6. **create_form_template** - Help create new form templates
7. **search_content** - Search for specific content on pages

## FLOWMANAGER CONTEXT:
- Blazor Server application for workflow and form management
- User roles: Admin, Moderator, BasicUser
- Main sections: Admin Dashboard, Form Templates, My Forms, Available Forms
- Form components: Text Input, Textarea, Radio Buttons, Checkboxes, Date Picker, File Upload
- Workflow process: Form Creation → Submission → Review → Approval

## RESPONSE FORMAT:
When you need to perform actions, respond with:
1. **Explanation** of what you'll do
2. **Action calls** using the available functions
3. **Confirmation** of what happened

Always explain what you're doing and why, then execute the actions.";

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                var recentHistory = _conversationHistory.TakeLast(10).ToList();
                foreach (var historyMessage in recentHistory)
                {
                    messages.Add(new
                    {
                        role = historyMessage.IsFromUser ? "user" : "assistant",
                        content = historyMessage.Content
                    });
                }

                messages.Add(new { role = "user", content = message });

                var requestBody = new
                {
                    model = _configuration["GoogleAI:Model"] ?? "gemini-1.5-flash",
                    messages = messages.ToArray(),
                    max_tokens = int.Parse(_configuration["GoogleAI:MaxTokens"] ?? "2000"),
                    temperature = double.Parse(_configuration["GoogleAI:Temperature"] ?? "0.3"),
                    functions = GetAvailableFunctions(),
                    function_call = "auto"
                };

                var historyForServer = _conversationHistory.Select(msg => new ChatMessageHistory
                {
                    Content = msg.Content,
                    IsFromUser = msg.IsFromUser,
                    Timestamp = msg.Timestamp
                }).ToList();

                var agentRequest = new AgentRequest
                {
                    Message = message,
                    SessionId = _sessionId,
                    ConversationHistory = historyForServer
                };

                var json = JsonSerializer.Serialize(agentRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/chat/agent", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[ChatService] Server response: {responseContent}");
                    
                    var agentResponse = JsonSerializer.Deserialize<AgentResponse>(responseContent);
                    Console.WriteLine($"[ChatService] Deserialized response: Content='{agentResponse?.Content}', FunctionCall='{agentResponse?.FunctionCall?.Name}'");
                    Console.WriteLine($"[ChatService] FunctionCall object: {agentResponse?.FunctionCall}");
                    Console.WriteLine($"[ChatService] FunctionCall Name: '{agentResponse?.FunctionCall?.Name}'");
                    Console.WriteLine($"[ChatService] FunctionCall Arguments: '{agentResponse?.FunctionCall?.Arguments}'");

                    if (agentResponse != null)
                    {
                        if (agentResponse.FunctionCall != null && !string.IsNullOrEmpty(agentResponse.FunctionCall.Name))
                        {
                            var functionArgs = new Dictionary<string, object>();
                            if (!string.IsNullOrEmpty(agentResponse.FunctionCall.Arguments))
                            {
                                try
                                {
                                    var argsJson = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(agentResponse.FunctionCall.Arguments);
                                    if (argsJson != null)
                                    {
                                        foreach (var kvp in argsJson)
                                        {
                                            functionArgs[kvp.Key] = kvp.Value.GetString() ?? "";
                                        }
                                    }
                                }
                                catch (Exception argEx)
                                {
                                    Console.WriteLine($"[ChatService] Error parsing function arguments: {argEx.Message}");
                                }
                            }

                            var agentAction = new AgentAction
                            {
                                FunctionName = agentResponse.FunctionCall.Name,
                                Parameters = functionArgs
                            };

                            var functionResult = await ExecuteFunction(agentAction);

                            return new ChatResponse
                            {
                                Content = functionResult,
                                IsMarkdown = true,
                                SessionId = _sessionId,
                                IsSuccessful = true
                            };
                        }
                        else
                        {
                            Console.WriteLine($"[ChatService] AI Agent response generated");

                            return new ChatResponse
                            {
                                Content = agentResponse.Content ?? "I'm not sure how to help with that.",
                                IsMarkdown = true,
                                SessionId = _sessionId,
                                IsSuccessful = true
                            };
                        }
                    }
                    else
                    {
                        throw new Exception("Failed to deserialize server response");
                    }
                }
                else
                {
                    Console.WriteLine($"[ChatService] Server API error: {response.StatusCode}");
                    throw new Exception($"Server API error: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatService] AI Agent error: {ex.Message}");

                return new ChatResponse
                {
                    Content = $"I encountered an error while processing your request: {ex.Message}",
                    IsMarkdown = false,
                    SessionId = _sessionId,
                    IsSuccessful = false
                };
            }
        }

        private Dictionary<string, Func<Dictionary<string, object>, Task<string>>> InitializeAgentActions()
        {
            return new Dictionary<string, Func<Dictionary<string, object>, Task<string>>>
            {
                ["navigate_to_page"] = NavigateToPage,
                ["fill_form_field"] = FillFormField,
                ["click_button"] = ClickButton,
                ["submit_form"] = SubmitForm,
                ["get_page_info"] = GetPageInfo,
                ["create_form_template"] = CreateFormTemplate,
                ["search_content"] = SearchContent
            };
        }

        private object[] GetAvailableFunctions()
        {
            return new object[]
            {
                new
                {
                    name = "navigate_to_page",
                    description = "Navigate to a specific page in the FlowManager application",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            page_url = new { type = "string", description = "The URL or route to navigate to" },
                            page_name = new { type = "string", description = "Human readable name of the page" }
                        },
                        required = new[] { "page_url" }
                    }
                },
                new
                {
                    name = "fill_form_field",
                    description = "Fill a specific field in a form with provided value",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            field_name = new { type = "string", description = "Name or identifier of the form field" },
                            value = new { type = "string", description = "Value to fill in the field" },
                            field_type = new { type = "string", description = "Type of field (text, email, date, etc.)" }
                        },
                        required = new[] { "field_name", "value" }
                    }
                },
                new
                {
                    name = "click_button",
                    description = "Click a button or interactive element on the page",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            button_text = new { type = "string", description = "Text displayed on the button" },
                            button_id = new { type = "string", description = "ID of the button element" },
                            action_type = new { type = "string", description = "Type of action (submit, save, cancel, etc.)" }
                        },
                        required = new[] { "button_text" }
                    }
                },
                new
                {
                    name = "get_page_info",
                    description = "Get information about the current page content and available elements",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            info_type = new { type = "string", description = "Type of information to get (forms, buttons, content, etc.)" }
                        }
                    }
                }
            };
        }

        private async Task<string> ExecuteFunction(AgentAction action)
        {
            try
            {
                Console.WriteLine($"[ChatService] Executing function: {action.FunctionName}");

                if (_availableActions.ContainsKey(action.FunctionName))
                {
                    var result = await _availableActions[action.FunctionName](action.Parameters ?? new Dictionary<string, object>());

                    Console.WriteLine($"[ChatService] Function {action.FunctionName} executed successfully");
                    return result;
                }
                else
                {
                    return $"Unknown function: {action.FunctionName}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChatService] Error executing function {action.FunctionName}: {ex.Message}");
                return $"Error executing function {action.FunctionName}: {ex.Message}";
            }
        }

        private async Task<string> NavigateToPage(Dictionary<string, object> parameters)
        {
            try
            {
                if (_jsRuntime == null)
                    return "Navigation is not available - JavaScript runtime not initialized.";

                var pageUrl = parameters.GetValueOrDefault("page_url")?.ToString() ?? "";
                var pageName = parameters.GetValueOrDefault("page_name")?.ToString() ?? pageUrl;

                await _jsRuntime.InvokeVoidAsync("navigateToPage", pageUrl);

                return $"Successfully navigated to {pageName} page.";
            }
            catch (Exception ex)
            {
                return $"Failed to navigate to page: {ex.Message}";
            }
        }

        private async Task<string> FillFormField(Dictionary<string, object> parameters)
        {
            try
            {
                if (_jsRuntime == null)
                    return "Form filling is not available - JavaScript runtime not initialized.";

                var fieldName = parameters.GetValueOrDefault("field_name")?.ToString() ?? "";
                var value = parameters.GetValueOrDefault("value")?.ToString() ?? "";
                var fieldType = parameters.GetValueOrDefault("field_type")?.ToString() ?? "text";

                var success = await _jsRuntime.InvokeAsync<bool>("fillFormField", fieldName, value, fieldType);

                if (success)
                    return $"Successfully filled field {fieldName} with value: {value}";
                else
                    return $"Could not find or fill field: {fieldName}";
            }
            catch (Exception ex)
            {
                return $"Failed to fill form field: {ex.Message}";
            }
        }

        private async Task<string> ClickButton(Dictionary<string, object> parameters)
        {
            try
            {
                if (_jsRuntime == null)
                    return "Button clicking is not available - JavaScript runtime not initialized.";

                var buttonText = parameters.GetValueOrDefault("button_text")?.ToString() ?? "";
                var buttonId = parameters.GetValueOrDefault("button_id")?.ToString() ?? "";
                var actionType = parameters.GetValueOrDefault("action_type")?.ToString() ?? "click";

                var success = await _jsRuntime.InvokeAsync<bool>("clickButton", buttonText, buttonId, actionType);

                if (success)
                    return $"Successfully clicked button: {buttonText}";
                else
                    return $"Could not find or click button: {buttonText}";
            }
            catch (Exception ex)
            {
                return $"Failed to click button: {ex.Message}";
            }
        }

        private async Task<string> SubmitForm(Dictionary<string, object> parameters)
        {
            try
            {
                if (_jsRuntime == null)
                    return "Form submission is not available - JavaScript runtime not initialized.";

                var success = await _jsRuntime.InvokeAsync<bool>("submitCurrentForm");

                if (success)
                    return "Form submitted successfully! You should receive a confirmation shortly.";
                else
                    return "Could not submit form. Please check that all required fields are filled.";
            }
            catch (Exception ex)
            {
                return $"Failed to submit form: {ex.Message}";
            }
        }

        private async Task<string> GetPageInfo(Dictionary<string, object> parameters)
        {
            try
            {
                if (_jsRuntime == null)
                    return "Page information is not available - JavaScript runtime not initialized.";

                var infoType = parameters.GetValueOrDefault("info_type")?.ToString() ?? "general";
                var pageInfo = await _jsRuntime.InvokeAsync<string>("getPageInfo", infoType);

                return $"Current page information:\n{pageInfo}";
            }
            catch (Exception ex)
            {
                return $"Failed to get page information: {ex.Message}";
            }
        }

        private async Task<string> CreateFormTemplate(Dictionary<string, object> parameters)
        {
            return "Form template creation functionality will be implemented in the next version. For now, please use the Admin Dashboard → Form Templates → Add Template manually.";
        }

        private async Task<string> SearchContent(Dictionary<string, object> parameters)
        {
            try
            {
                if (_jsRuntime == null)
                    return "Content search is not available - JavaScript runtime not initialized.";

                var searchTerm = parameters.GetValueOrDefault("search_term")?.ToString() ?? "";
                var results = await _jsRuntime.InvokeAsync<string>("searchPageContent", searchTerm);

                return $"Search results for '{searchTerm}':\n{results}";
            }
            catch (Exception ex)
            {
                return $"Failed to search content: {ex.Message}";
            }
        }

        public void ClearSession()
        {
            _conversationHistory.Clear();
            Console.WriteLine("[ChatService] Session cleared");
        }

        public string GetSessionId()
        {
            return _sessionId;
        }
    }

    public class OpenAIResponse
    {
        public Choice[]? choices { get; set; }
    }

    public class Choice
    {
        public Message? message { get; set; }
    }

    public class Message
    {
        public string? content { get; set; }
        public FunctionCall? function_call { get; set; }
    }

    public class FunctionCall
    {
        public string name { get; set; } = "";
        public string? arguments { get; set; }
    }

    public class ChatResponse
    {
        public string Content { get; set; } = "";
        public bool IsMarkdown { get; set; }
        public string SessionId { get; set; } = "";
        public bool IsSuccessful { get; set; }
    }

    public class AgentRequest
    {
        public string Message { get; set; } = "";
        public string SessionId { get; set; } = "";
        public List<ChatMessageHistory>? ConversationHistory { get; set; }
    }

    public class AgentResponse
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
        
        [JsonPropertyName("functionCall")]
        public FunctionCallInfo? FunctionCall { get; set; }
    }

    public class ChatMessageHistory
    {
        public string Content { get; set; } = "";
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class FunctionCallInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        [JsonPropertyName("arguments")]
        public string? Arguments { get; set; }
    }

    public class AgentAction
    {
        public string FunctionName { get; set; } = "";
        public Dictionary<string, object> Parameters { get; set; } = new();
    }

}