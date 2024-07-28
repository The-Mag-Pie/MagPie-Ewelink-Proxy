using System.Security.Cryptography;
using System.Text;

namespace MagPie_Ewelink_Proxy
{
    public static class Utils
    {
        public static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] randomString = new char[length];
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                randomString[i] = chars[random.Next(chars.Length)];
            }

            return new string(randomString);
        }

        public static string CalculateHMACSignature(string data, string key)
        {
            var encoding = new ASCIIEncoding();
            var keyBytes = encoding.GetBytes(key);
            var dataBytes = encoding.GetBytes(data);

            using var hmacsha256 = new HMACSHA256(keyBytes);
            var hash = hmacsha256.ComputeHash(dataBytes);

            return Convert.ToBase64String(hash);
        }
    }
}
