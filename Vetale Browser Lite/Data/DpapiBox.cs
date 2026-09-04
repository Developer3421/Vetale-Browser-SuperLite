using System;
using System.Security.Cryptography;
using System.Text;

namespace Vetale_Browser_Lite.Data
{
    /// <summary>
    /// Value encryption with NO key file: DPAPI in CurrentUser scope.
    /// The key is managed by Windows per Windows user and never touches
    /// disk in our files — another user/machine cannot decrypt the data.
    /// </summary>
    internal static class DpapiBox
    {
        private static readonly byte[] Entropy =
            Encoding.UTF8.GetBytes("VetaleBrowserSuperLite:v1:dpapi-entropy");

        public static string Protect(string plainText)
        {
            var plain = Encoding.UTF8.GetBytes(plainText ?? string.Empty);
            var cipher = ProtectedData.Protect(plain, Entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(cipher);
        }

        public static string Unprotect(string protectedBase64)
        {
            var cipher = Convert.FromBase64String(protectedBase64 ?? string.Empty);
            var plain = ProtectedData.Unprotect(cipher, Entropy, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(plain);
        }
    }
}
