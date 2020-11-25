using System;

namespace Geta.Bring.Common.Model
{
    public class AuthenticationSettings : IAuthenticationSettings
    {
        public Uri ClientUri { get; set; }

        public string Uid { get; set; }

        public string Key { get; set; }
    }
}
