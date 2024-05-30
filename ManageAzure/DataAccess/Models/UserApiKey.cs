using System;
using System.Collections.Generic;

namespace ManageAzure.DataAccess.Models
{
    public partial class UserApiKey
    {
        public string Email { get; set; } = null!;
        public string ApiKey { get; set; } = null!;
    }
}
