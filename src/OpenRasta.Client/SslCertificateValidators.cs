using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace OpenRasta.Client
{
    /// <summary>
    /// Contains validation callbacks for
    /// <see cref="RemoteCertificateValidationCallback"/>.
    /// </summary>
    public static class SslCertificateValidators
    {
        /// <summary>
        /// Validates any SSL certificate, however invalid.
        /// </summary>
        /// <param name="sender">An object that contains state information for this validation.</param>
        /// <param name="certificate">The certificate used to authenticate the remote party.</param>
        /// <param name="chain">The chain of certificate authorities associated with the remote certificate.</param>
        /// <param name="policyErrors">One or more errors associated with the remote certificate.</param>
        /// <returns><code>true</code></returns>
        public static bool ValidateAnyRemoteCertificate(
                object sender,
                X509Certificate certificate,
                X509Chain chain,
                SslPolicyErrors sslPolicyErrors)
        {
            // allow any SSL certificate (self-signed, expired, ...)
            return true;
        }
    }
}
