using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Helpers
{
    public static class HttpCertificateValidationHelper
    {
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isValidCertificate = false;

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                isValidCertificate = true;
            }
            else
            {                 
                Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
                return false;
            }

            return isValidCertificate;
        }
    }
}
