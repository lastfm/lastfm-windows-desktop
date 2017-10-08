using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LastFM.ApiClient.Helpers
{
    // Certifate validation routine to ensure we are only talking with the endpoint we think we are
    public static class HttpCertificateValidationHelper
    {
        // Method used to implement certificate validation
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // Default to the certificate being invalid
            bool isValidCertificate = false;

            // If there are no known errors on the certificate
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                // Then it's valid
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
