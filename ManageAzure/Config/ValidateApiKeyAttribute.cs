using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using ManageAzure.Interfaces;

namespace ManageAzure.Config
{
    public class ValidateApiKeyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var serviceProvider = context.HttpContext.RequestServices;
            var _apiKeyService = serviceProvider.GetService<IApiKeyRepository>() ?? throw new Exception();

            string apiKey = context.HttpContext.Request.Headers["X-API-Key"].ToString();

            if (!_apiKeyService.ValidateApiKey(apiKey).Result)
            {
                context.Result = new UnauthorizedObjectResult("Invalid API key or email.");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
