namespace ManageAzure.Config
{
    public class AzureDevOpsOptions
    {
        public string Organization { get; set; }
        public string Project { get; set; }
        public string PersonalAccessToken { get; set; }
        public string ApiVersion { get; set; }
        public string Url { get; set; }
        public AzureDevOpsOptionsEndpoints? Endpoints { get; set; }
    }

    public class AzureDevOpsOptionsEndpoints
    {
        public string UploadFile { get; set; }
        public string LinkFile { get; set; }
    }
}
