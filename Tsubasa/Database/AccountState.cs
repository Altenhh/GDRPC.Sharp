using System.Security.Cryptography;
using System.Text;

namespace Tsubasa.Database
{
    public class AccountState
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public static byte[] CreateMd5Hash(string password)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(password);

                return md5.ComputeHash(inputBytes);
            }
        }
    }
}