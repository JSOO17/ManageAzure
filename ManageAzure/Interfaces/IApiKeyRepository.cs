namespace ManageAzure.Interfaces
{
    public interface IApiKeyRepository
    {
        Task<bool> ValidateApiKey(string apiKey);
    }
}
