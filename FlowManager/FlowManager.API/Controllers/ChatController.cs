using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Text;

namespace FlowManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ChatController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpPost("agent")]
        public async Task<IActionResult> ProcessAgentRequest([FromBody] ChatAgentRequest request)
        {
            try
            {
                var googleAiApiKey = _configuration["GoogleAI:ApiKey"];
                
                if (string.IsNullOrEmpty(googleAiApiKey) || googleAiApiKey == "your-google-ai-api-key-here")
                {
                    return BadRequest(new { error = "Google AI API key not configured" });
                }

                var systemPrompt = @"You are FlowManager AI Agent. You MUST perform actions when users request them, not just explain what you would do.

## CORE BEHAVIOR:
- When user says ""click on X"" or ""apasă pe X"" → YOU MUST CLICK THE BUTTON
- When user says ""go to admin"" or ""mergi la admin"" → YOU MUST NAVIGATE 
- When user says ""fill form"" or ""completează formularul"" → YOU MUST FILL FORMS
- Always ACT first, then explain what you did

## AVAILABLE ACTIONS:
1. **navigate_to_page** - Navigate to pages (/admin, /basic-user, etc.)
2. **click_button** - Click any button (Save, Submit, My Details, etc.)
3. **fill_form_field** - Fill form fields
4. **get_page_info** - Get page information
5. **submit_form** - Submit forms
6. **search_content** - Search content

## EXAMPLES:
User: ""poti sa apesi pe butonul My Details?""
Response: ""Da, apas acum pe butonul My Details."" 
Action: click_button with ""My Details""

User: ""mergi pe pagina de admin""
Response: ""Navighez acum către pagina de admin.""
Action: navigate_to_page with ""/admin""

ALWAYS PERFORM THE ACTION when requested. Be proactive and helpful.";

                var messages = new List<object>
                {
                    new { role = "system", content = systemPrompt }
                };

                if (request.ConversationHistory != null)
                {
                    foreach (var historyMessage in request.ConversationHistory)
                    {
                        messages.Add(new { 
                            role = historyMessage.IsFromUser ? "user" : "assistant", 
                            content = historyMessage.Content 
                        });
                    }
                }

                messages.Add(new { role = "user", content = request.Message });

                var functions = new object[]
                {
                    new
                    {
                        name = "navigate_to_page",
                        description = "Navigate to a specific page in the application",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                page_name = new { type = "string", description = "The page to navigate to (forms, admin, dashboard, etc.)" }
                            },
                            required = new[] { "page_name" }
                        }
                    },
                    new
                    {
                        name = "fill_form_field",
                        description = "Fill a specific form field with a value",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                field_name = new { type = "string", description = "The name or ID of the field to fill" },
                                value = new { type = "string", description = "The value to put in the field" }
                            },
                            required = new[] { "field_name", "value" }
                        }
                    },
                    new
                    {
                        name = "click_button",
                        description = "Click a button or link on the page",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                button_text = new { type = "string", description = "The text or identifier of the button to click" }
                            },
                            required = new[] { "button_text" }
                        }
                    },
                    new
                    {
                        name = "submit_form",
                        description = "Submit the current form",
                        parameters = new
                        {
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        }
                    },
                    new
                    {
                        name = "get_page_info",
                        description = "Get information about the current page",
                        parameters = new
                        {
                            type = "object",
                            properties = new { },
                            required = new string[] { }
                        }
                    },
                    new
                    {
                        name = "create_form_template",
                        description = "Create a new form template",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                template_name = new { type = "string", description = "The name of the template" },
                                description = new { type = "string", description = "Description of what the template is for" }
                            },
                            required = new[] { "template_name", "description" }
                        }
                    },
                    new
                    {
                        name = "search_content",
                        description = "Search for specific content on the current page",
                        parameters = new
                        {
                            type = "object",
                            properties = new
                            {
                                search_term = new { type = "string", description = "The term to search for" }
                            },
                            required = new[] { "search_term" }
                        }
                    }
                };

                var geminiContents = new List<object>();
                string extractedSystemPrompt = "";
                
                foreach (var message in messages)
                {
                    var messageObj = message as dynamic;
                    if (messageObj?.role == "system")
                    {
                        extractedSystemPrompt = messageObj?.content?.ToString() ?? "";
                        continue;
                    }
                    
                    var role = messageObj?.role == "user" ? "user" : "model";
                    var messageContent = messageObj?.content?.ToString() ?? "";
                    
                    geminiContents.Add(new
                    {
                        role = role,
                        parts = new[] { new { text = messageContent } }
                    });
                }

                if (!string.IsNullOrEmpty(extractedSystemPrompt) && geminiContents.Count > 0)
                {
                    var firstMessage = geminiContents[0];
                    var firstMessageObj = firstMessage as dynamic;
                    if (firstMessageObj?.role == "user")
                    {
                        var originalText = ((firstMessageObj?.parts as object[])?[0] as dynamic)?.text?.ToString() ?? "";
                        geminiContents[0] = new
                        {
                            role = "user",
                            parts = new[] { new { text = $"{extractedSystemPrompt}\n\n{originalText}" } }
                        };
                    }
                }

                Console.WriteLine($"[ChatController] Gemini contents count: {geminiContents.Count}");
                Console.WriteLine($"[ChatController] System prompt length: {extractedSystemPrompt.Length}");

                var requestBody = new
                {
                    contents = geminiContents.ToArray(),
                    generationConfig = new
                    {
                        temperature = double.TryParse(_configuration["GoogleAI:Temperature"], out var temperature) ? temperature : 0.7,
                        maxOutputTokens = int.TryParse(_configuration["GoogleAI:MaxTokens"], out var maxTokens) ? maxTokens : 2000
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                Console.WriteLine($"[ChatController] Sending to Gemini API: {json}");

                _httpClient.DefaultRequestHeaders.Clear();
                
                var geminiModel = _configuration["GoogleAI:Model"] ?? "gemini-1.5-flash";
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/{geminiModel}:generateContent?key={googleAiApiKey}";
                
                Console.WriteLine($"[ChatController] API URL: {apiUrl}");
                
                var response = await _httpClient.PostAsync(apiUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Google AI API Error: {response.StatusCode} - {responseContent}");
                    return StatusCode(500, new { error = "AI service error", details = responseContent });
                }

                Console.WriteLine($"[ChatController] Gemini API Response: {responseContent}");

                var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseContent);
                var responseText = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text;

                Console.WriteLine($"[ChatController] Parsed response text: {responseText}");

                var functionCall = ParseGeminiFunctionCall(responseText);
                
                Console.WriteLine($"[ChatController] Detected function call: {functionCall?.Name}");

                return Ok(new ChatAgentResponse
                {
                    Content = responseText ?? "I'm not sure how to help with that. Could you please rephrase your request?",
                    FunctionCall = functionCall
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chat API Error: {ex.Message}");
                return StatusCode(500, new { error = "Internal server error", message = ex.Message });
            }
        }

        private FunctionCallInfo? ParseGeminiFunctionCall(string? responseText)
        {
            if (string.IsNullOrEmpty(responseText))
                return null;

            var lowerText = responseText.ToLowerInvariant();
            Console.WriteLine($"[ChatController] Parsing text: {lowerText}");

            if (lowerText.Contains("intru pe") || lowerText.Contains("navighez") || lowerText.Contains("mergi la") || 
                lowerText.Contains("go to") || lowerText.Contains("navigate") || lowerText.Contains("deschid") ||
                lowerText.Contains("poti sa mergi"))
            {
                if (lowerText.Contains("admin") || lowerText.Contains("administrare"))
                {
                    return new FunctionCallInfo
                    {
                        Name = "navigate_to_page",
                        Arguments = JsonSerializer.Serialize(new { page_url = "/admin", page_name = "admin" })
                    };
                }
                else if (lowerText.Contains("form") || lowerText.Contains("formulare"))
                {
                    return new FunctionCallInfo
                    {
                        Name = "navigate_to_page", 
                        Arguments = JsonSerializer.Serialize(new { page_url = "/basic-user", page_name = "forms" })
                    };
                }
                else if (lowerText.Contains("dashboard") || lowerText.Contains("tablou"))
                {
                    return new FunctionCallInfo
                    {
                        Name = "navigate_to_page",
                        Arguments = JsonSerializer.Serialize(new { page_url = "/dashboard", page_name = "dashboard" })
                    };
                }
                else if (lowerText.Contains("home") || lowerText.Contains("acasă") || lowerText.Contains("principal"))
                {
                    return new FunctionCallInfo
                    {
                        Name = "navigate_to_page",
                        Arguments = JsonSerializer.Serialize(new { page_url = "/", page_name = "home" })
                    };
                }
            }

            if (lowerText.Contains("apas") || lowerText.Contains("click") || lowerText.Contains("buton") ||
                lowerText.Contains("poti sa apesi"))
            {
                string buttonText = "";
                
                if (lowerText.Contains("my details"))
                {
                    buttonText = "My Details";
                }
                else if (lowerText.Contains("save") || lowerText.Contains("salv"))
                {
                    buttonText = "Save";
                }
                else if (lowerText.Contains("submit") || lowerText.Contains("trimite"))
                {
                    buttonText = "Submit";
                }
                else if (lowerText.Contains("add") || lowerText.Contains("adaug"))
                {
                    buttonText = "Add";
                }
                else if (lowerText.Contains("edit") || lowerText.Contains("modific"))
                {
                    buttonText = "Edit";
                }
                else if (lowerText.Contains("delete") || lowerText.Contains("șterge"))
                {
                    buttonText = "Delete";
                }
                else
                {
                    var patterns = new[]
                    {
                        @"butonul\s+(.+?)[\s\.]",
                        @"button\s+(.+?)[\s\.]",
                        @"apesi pe\s+(.+?)[\s\.]",
                        @"click.*?pe\s+(.+?)[\s\.]"
                    };

                    foreach (var pattern in patterns)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(lowerText, pattern);
                        if (match.Success)
                        {
                            buttonText = match.Groups[1].Value.Trim();
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(buttonText))
                {
                    Console.WriteLine($"[ChatController] Detected button click: {buttonText}");
                    return new FunctionCallInfo
                    {
                        Name = "click_button",
                        Arguments = JsonSerializer.Serialize(new { button_text = buttonText })
                    };
                }
            }

            if (lowerText.Contains("completz") || lowerText.Contains("fill") || lowerText.Contains("introduc"))
            {
                return new FunctionCallInfo
                {
                    Name = "get_page_info",
                    Arguments = JsonSerializer.Serialize(new { info_type = "forms" })
                };
            }

            Console.WriteLine($"[ChatController] No action detected for text: {lowerText}");
            return null;
        }
    }

    public class ChatAgentRequest
    {
        public string Message { get; set; } = "";
        public string SessionId { get; set; } = "";
        public List<ChatMessageHistory>? ConversationHistory { get; set; }
    }

    public class ChatMessageHistory
    {
        public string Content { get; set; } = "";
        public bool IsFromUser { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ChatAgentResponse
    {
        public string? Content { get; set; }
        public FunctionCallInfo? FunctionCall { get; set; }
    }

    public class FunctionCallInfo
    {
        public string Name { get; set; } = "";
        public string? Arguments { get; set; }
    }

    public class GeminiApiResponse
    {
        public GeminiCandidate[]? candidates { get; set; }
    }

    public class GeminiCandidate
    {
        public GeminiContent? content { get; set; }
    }

    public class GeminiContent
    {
        public GeminiPart[]? parts { get; set; }
    }

    public class GeminiPart
    {
        public string? text { get; set; }
    }
}