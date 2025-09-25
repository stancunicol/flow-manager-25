using FlowManager.Client.DTOs.Chat;
using FlowManager.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text.RegularExpressions;

namespace FlowManager.Client.Components.Chatbot
{
    public partial class Chatbot : ComponentBase, IDisposable
    {
        [Inject] private ChatService ChatService { get; set; } = default!;
        [Inject] private IJSRuntime JsRuntime { get; set; } = default!;

        [Parameter] public bool AutoOpen { get; set; } = false;
        [Parameter] public string? InitialMessage { get; set; }
        [Parameter] public string Position { get; set; } = "bottom-right"; 

        private bool IsOpen { get; set; } = false;
        private bool IsTyping { get; set; } = false;
        private string CurrentMessage { get; set; } = string.Empty;
        private int UnreadCount { get; set; } = 0;
        private bool IsAIEnabled { get; set; } = false;
        
        private List<ChatMessage> Messages { get; set; } = new();
        private ElementReference messagesContainer;
        private ElementReference inputElement;

        private Timer? typingTimer;
        private readonly object lockObject = new object();

        protected override async Task OnInitializedAsync()
        {
            await ChatService.InitializeAsync();
            
            IsAIEnabled = true; 

            if (AutoOpen)
            {
                IsOpen = true;
            }

            if (!string.IsNullOrWhiteSpace(InitialMessage))
            {
                await SendQuickMessage(InitialMessage);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                ChatService.SetJSRuntime(JsRuntime);
                
                if (IsOpen)
                {
                    await FocusInput();
                }
            }

            if (IsOpen)
            {
                await ScrollToBottom();
            }
        }

        private async Task ToggleChatbot()
        {
            IsOpen = !IsOpen;

            if (IsOpen)
            {
                UnreadCount = 0;
                await FocusInput();
                await ScrollToBottom();
            }

            StateHasChanged();
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(CurrentMessage) || IsTyping)
                return;

            var userMessage = CurrentMessage.Trim();
            CurrentMessage = string.Empty;

            Messages.Add(new ChatMessage
            {
                Content = userMessage,
                IsFromUser = true,
                Timestamp = DateTime.Now,
                IsMarkdown = false
            });

            StateHasChanged();
            await ScrollToBottom();

            IsTyping = true;
            StateHasChanged();

            try
            {
                await Task.Delay(1000);

                var response = await ChatService.SendMessageAsync(userMessage);

                IsTyping = false;

                if (response != null)
                {
                    Messages.Add(new ChatMessage
                    {
                        Content = response.Content,
                        IsFromUser = false,
                        Timestamp = DateTime.Now,
                        IsMarkdown = response.IsMarkdown
                    });

                    if (!IsOpen)
                    {
                        UnreadCount++;
                    }
                }
                else
                {
                    Messages.Add(new ChatMessage
                    {
                        Content = "Sorry, I'm having trouble responding right now. Please try again in a moment.",
                        IsFromUser = false,
                        Timestamp = DateTime.Now,
                        IsMarkdown = false
                    });
                }
            }
            catch (Exception ex)
            {
                IsTyping = false;
                Console.WriteLine($"[Chatbot] Error sending message: {ex.Message}");

                Messages.Add(new ChatMessage
                {
                    Content = "I encountered an error while processing your message. Please try again.",
                    IsFromUser = false,
                    Timestamp = DateTime.Now,
                    IsMarkdown = false
                });
            }

            StateHasChanged();
            await ScrollToBottom();
            await FocusInput();
        }

        private async Task SendQuickMessage(string message)
        {
            CurrentMessage = message;
            await SendMessage();
        }

        private void SetMessage(string message)
        {
            CurrentMessage = message;
            StateHasChanged();
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (e.Key == "Enter" && !e.ShiftKey)
            {
                await SendMessage();
            }
        }

        private void HandleInputChange(ChangeEventArgs e)
        {
            CurrentMessage = e.Value?.ToString() ?? string.Empty;
            StateHasChanged();
        }

        private async Task ScrollToBottom()
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("scrollToBottom", messagesContainer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Chatbot] Error scrolling to bottom: {ex.Message}");
            }
        }

        private async Task FocusInput()
        {
            try
            {
                await Task.Delay(100); 
                await inputElement.FocusAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Chatbot] Error focusing input: {ex.Message}");
            }
        }

        private string ConvertMarkdownToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return string.Empty;


            var html = markdown;

            html = Regex.Replace(html, @"\*\*(.*?)\*\*", "<strong>$1</strong>");

            html = Regex.Replace(html, @"\*(.*?)\*", "<em>$1</em>");

            html = Regex.Replace(html, @"`(.*?)`", "<code>$1</code>");

            html = html.Replace("\n", "<br>");

            html = Regex.Replace(html, @"^\d+\.\s(.+)$", "<li>$1</li>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"(<li>.*</li>)", "<ol>$1</ol>", RegexOptions.Singleline);

            html = Regex.Replace(html, @"^[-\*]\s(.+)$", "<li>$1</li>", RegexOptions.Multiline);
            html = Regex.Replace(html, @"(<li>.*</li>)", "<ul>$1</ul>", RegexOptions.Singleline);

            if (!html.StartsWith("<"))
            {
                html = $"<p>{html}</p>";
            }

            return html;
        }

        public void ClearMessages()
        {
            Messages.Clear();
            StateHasChanged();
        }

        public async Task SendSystemMessage(string message)
        {
            Messages.Add(new ChatMessage
            {
                Content = message,
                IsFromUser = false,
                Timestamp = DateTime.Now,
                IsMarkdown = false
            });

            if (!IsOpen)
            {
                UnreadCount++;
            }

            StateHasChanged();
            await ScrollToBottom();
        }

        public void Dispose()
        {
            typingTimer?.Dispose();
        }
    }
}