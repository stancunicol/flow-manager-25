
using FlowManager.Shared.DTOs.Responses.FormReview;
using FlowManager.Client.DTOs;
using System.Text.Json;
using System.Web;
using System.Net.Http.Json;

namespace FlowManager.Client.Services
{
    public class FormReviewService
    {
        private readonly HttpClient _httpClient;

        public FormReviewService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PagedReviewHistoryResponse?> GetReviewHistoryByModeratorAsync(
            Guid moderatorId,
            int page,
            int pageSize,
            string? searchTerm = null,
            string? actionFilter = null)
        {
            try
            {
                var uriBuilder = new UriBuilder(_httpClient.BaseAddress!)
                {
                    Path = $"api/formreviews/moderator/{moderatorId}/history"
                };

                var query = HttpUtility.ParseQueryString(string.Empty);

                if (!string.IsNullOrEmpty(searchTerm))
                    query["SearchTerm"] = searchTerm;

                if (!string.IsNullOrEmpty(actionFilter))
                    query["Action"] = actionFilter;

                query["QueryParams.Page"] = page.ToString();
                query["QueryParams.PageSize"] = pageSize.ToString();
                query["QueryParams.SortBy"] = "ReviewedAt";
                query["QueryParams.SortDescending"] = "true";

                uriBuilder.Query = query.ToString();

                Console.WriteLine($"[FormReviewService] Request URL: {uriBuilder.Uri}");

                var response = await _httpClient.GetAsync(uriBuilder.Uri);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<FormReviewApiResponse>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Result != null)
                    {
                        var reviews = apiResponse.Result.Data != null ?
                            new List<FormReviewResponseDto>(apiResponse.Result.Data) :
                            new List<FormReviewResponseDto>();

                        return new PagedReviewHistoryResponse
                        {
                            Reviews = reviews,
                            TotalCount = apiResponse.Result.TotalCount,
                            Page = apiResponse.Result.Page,
                            PageSize = apiResponse.Result.PageSize,
                            HasMore = apiResponse.Result.HasNextPage
                        };
                    }
                }
                else
                {
                    Console.WriteLine($"[FormReviewService] Request failed: {response.StatusCode}");
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting review history: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> ApproveFormAsync(Guid formResponseId, Guid moderatorId)
        {
            try
            {
                Console.WriteLine($"[FormReviewService] Approving form {formResponseId} by moderator {moderatorId}");

                var response = await _httpClient.PostAsJsonAsync($"api/formreviews/approve", new
                {
                    FormResponseId = formResponseId,
                    ModeratorId = moderatorId,
                    Action = "Approved",
                    Comments = ""
                });

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FormReviewService] Form approved successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FormReviewService] Failed to approve form: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FormReviewService] Error approving form: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> RejectFormAsync(Guid formResponseId, Guid moderatorId, string? comments = null)
        {
            try
            {
                Console.WriteLine($"[FormReviewService] Rejecting form {formResponseId} by moderator {moderatorId}");

                var response = await _httpClient.PostAsJsonAsync($"api/formreviews/reject", new
                {
                    FormResponseId = formResponseId,
                    ModeratorId = moderatorId,
                    Action = "Rejected",
                    Comments = comments ?? ""
                });

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[FormReviewService] Form rejected successfully");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FormReviewService] Failed to reject form: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FormReviewService] Error rejecting form: {ex.Message}");
                return false;
            }
        }
    }
}