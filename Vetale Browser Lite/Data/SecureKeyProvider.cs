using System;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Vetale_Browser_Lite.Data
{
    /// <summary>
    /// User-level key with NO key file: the database password is derived at
    /// runtime from the current Windows user SID + app salt and lives only
    /// in memory. Another Windows user (or another machine) gets a different
    /// key, so the .db files cannot be opened elsewhere.
    /// </summary>
    internal static class SecureKeyProvider
    {
        private const string Salt = "VetaleBrowserSuperLite:v1:user-db-key";
        private static string _password = string.Empty;

        public static string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_password))
                    _password = Derive();
                return _password;
            }
        }

        private static string Derive()
        {
            string sid;
            try
            {
                sid = WindowsIdentity.GetCurrent()?.User?.Value
                      ?? Environment.UserDomainName + "\\" + Environment.UserName;
            }
            catch
            {
                sid = Environment.UserDomainName + "\\" + Environment.UserName;
            }

            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(Salt + ":" + sid));
            return Convert.ToBase64String(bytes);
        }
    }
}
