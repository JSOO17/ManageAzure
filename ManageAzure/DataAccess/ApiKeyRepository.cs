using ManageAzure.DataAccess.Models;
using ManageAzure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ManageAzure.DataAccess
{
    public class ApiKeyRepository : IApiKeyRepository
    {
        private readonly productsContext _context;

        public ApiKeyRepository(productsContext context)
        {
            _context = context;
        }

        public async Task<bool> ValidateApiKey(string apiKey)
        {
            var user = await _context.UserApiKeys.SingleOrDefaultAsync(u => u.ApiKey == apiKey);
            return user != null;
        }
    }
}
