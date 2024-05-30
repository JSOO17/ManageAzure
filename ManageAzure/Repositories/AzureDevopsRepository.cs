using ManageAzure.Interfaces;
using ManageAzure.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using ManageAzure.Config;
using System.Text.Json.Serialization;
using System;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json.Linq;

namespace ManageAzure.Repositories
{
    public class AzureDevopsRepository : IAzureDevOpsRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<AzureDevOpsOptions> _options;
        private readonly ILogger<AzureDevopsRepository> _logger;
        private readonly IFilesRepository _filesRepository;

        public AzureDevopsRepository(IOptions<AzureDevOpsOptions> options, ILogger<AzureDevopsRepository> logger, IFilesRepository filesRepository)
        {
            _options = options;
            _logger = logger;
            _filesRepository = filesRepository;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_options.Value.PersonalAccessToken}")));
        }

        public async Task<string> UploadAttachmentAsync(byte[] fileContent, string fileName)
        {
            var config = _options.Value;
            var url = $"{config.Url}{config.Organization}/{config.Project}{config.Endpoints?.UploadFile}{fileName}&{config.ApiVersion}";

            using var content = new ByteArrayContent(fileContent);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync(url, content);

            if (response.StatusCode == System.Net.HttpStatusCode.Created)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var attachment = JsonSerializer.Deserialize<AzureDevOpsAttachmentResponse>(responseContent);
                return attachment?.Url ?? string.Empty;
            }
            else
            {
                _logger.LogError($"Failed to upload attachment: {response.ReasonPhrase}");
                return null;
            }
        }

        public async Task LinkAttachmentToWorkItemAsync(int workItemId, string attachmentUrl)
        {
            var config = _options.Value;
            var url = $"{config.Url}{config.Organization}/{config.Project}{config.Endpoints?.LinkFile}{workItemId}/?{config.ApiVersion}";

            var patchDocument = new[]
            {
                new
                {
                    op = "add",
                    path = "/relations/-",
                    value = new
                    {
                        rel = "AttachedFile",
                        url = attachmentUrl
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(patchDocument), Encoding.UTF8, "application/json");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

            var response = await _httpClient.PatchAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Attachment linked to work item {workItemId}");
            }
            else
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to link attachment to work item {workItemId}: {response.ReasonPhrase}");
            }
        }

        public async Task UploadFiles(FileModel file, int workItemId)
        {
            var url = await UploadAttachmentAsync(Convert.FromBase64String(file?.Content), file?.Name);

            await LinkAttachmentToWorkItemAsync(workItemId, url);
        }

        private string BuildFields(Ticket ticket)
        {
            return new JArray(
                new JObject
                {
                    { "op", "add" },
                    { "path", "/fields/System.Title" },
                    { "value", ticket.Title }
                },
                new JObject
                {
                    { "op", "add" },
                    { "path", "/fields/System.Description" },
                    { "value", ticket.Description }
                },
                new JObject
                {
                    { "op", "add" },
                    { "path", "/fields/System.AssignedTo" },
                    { "value", ticket.AssignedTo } 
                },
                new JObject
                {
                    { "op", "add" },
                    { "path", "/fields/Custom.Category" }, 
                    { "value", ticket.Category }
                }
            ).ToString();
        }

        public async Task<TicketResponse> CreateIssue(Ticket ticket)
        {
            var config = _options.Value;
            var uri = $"{config.Url}{config.Organization}/{config.Project}/_apis/wit/workitems/${"Issue"}?api-version=5.1";

            var fields = BuildFields(ticket);

            var content = new StringContent(fields, Encoding.UTF8, "application/json-patch+json");

            try
            {
                var response = await _httpClient.PostAsync(uri, content);

                response.EnsureSuccessStatusCode();

                using var responseStream = await response.Content.ReadAsStreamAsync();
                using var jsonDocument = await JsonDocument.ParseAsync(responseStream);
                var ticketId = jsonDocument.RootElement.GetProperty("id").GetInt32();

                return new TicketResponse
                {
                    Id = ticketId
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al crear el Work Item:");
                Console.WriteLine(ex.Message);

                throw;
            }
        }
    }

    public class AzureDevOpsAttachmentResponse
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
