using Geta.Bring.Common.Model;
using System.Collections.Generic;

namespace Geta.Bring.Common
{
    public class AuthenticationSettingsComparer : IEqualityComparer<IAuthenticationSettings>
    {
        public bool Equals(IAuthenticationSettings x, IAuthenticationSettings y)
        {
            if (x == null && y == null) return true;
            if (x == null) return false;
            if (y == null) return false;

            if (x.Key != y.Key) return false;
            if (x.Uid != y.Uid) return false;
            if (x.ClientUri != y.ClientUri) return false;

            return true;
        }

        public int GetHashCode(IAuthenticationSettings obj)
        {
            var code = 0;

            if (obj == null)
                return code;
           
            if (obj.Key != null)
                code += obj.Key.GetHashCode() * 17;

            if (obj.Uid != null)
                code += obj.Uid.GetHashCode() * 23;

            if (obj.ClientUri != null)
                code += obj.ClientUri.GetHashCode();

           return code;
        }
    }
}
