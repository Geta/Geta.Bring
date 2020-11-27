using System;

namespace Geta.Bring.Common.Model
{
    public interface IAuthenticationSettings
    {
        Uri ClientUri { get; }
        string Uid { get; }
        string Key { get; }
    }
}
