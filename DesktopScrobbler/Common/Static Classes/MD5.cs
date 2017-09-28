
using System.Security.Cryptography;
using System.Text;

namespace LastFM.Common.Static_Classes
{
    public static class MD5Helper
    {
        private static MD5 _md5Hasher = MD5.Create();

        internal static string GetHash(string sourceData)
        {
            byte[] sourceDataBytes = Encoding.UTF8.GetBytes(sourceData);
            byte[] hashBytes = _md5Hasher.ComputeHash(sourceDataBytes);

            return Encoding.UTF8.GetString(hashBytes);
        }
    }
}
