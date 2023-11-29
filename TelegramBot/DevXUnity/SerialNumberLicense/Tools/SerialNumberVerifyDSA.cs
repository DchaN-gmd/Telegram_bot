using System;
using System.Security.Cryptography;

namespace DevXUnity.SerialNumberLicense.Tools
{
#if UNITY_WSA
#else
    public sealed class SerialNumberVerifyDsa
    {
        private const string HashAlg = "SHA1";
        private readonly DSAParameters _publicKeyInfo;
        
        public SerialNumberVerifyDsa(string serializedKey)
        {
            if (string.IsNullOrEmpty(serializedKey)) return; 
            
            if (serializedKey.StartsWith("DSA:O:")) 
                _publicKeyInfo = Parce(serializedKey["DSA:O:".Length..]);
        }
        
        public bool VerifySignature(byte[] value, byte[] signature)
        {
            if (value == null || value.Length == 0 || 
                signature == null || signature.Length == 0) return false;
            return VerifyHash(value, signature, _publicKeyInfo);
        }
        
        static DSAParameters Parce(string s)
        {
            var val = new DSAParameters();
            if (string.IsNullOrEmpty(s)) return new DSAParameters();

            var l = s.Replace("\r\n", "\n").Split('\n');

            var i = 0;
            val.Counter = int.Parse(l[i]);
            i++;
            val.G = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]); 
            i++;
            val.J = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]); 
            i++;
            val.P = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]);
            i++;
            val.Q = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]);
            i++;
            val.Seed = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]); 
            i++;
            val.X = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]); 
            i++;
            val.Y = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]);

            return val;
        }

        private static bool VerifyHash(byte[] hashValue, byte[] signedHashValue, DSAParameters dsaKeyInfo)
        {
            if (hashValue == null || hashValue.Length == 0 || signedHashValue is not { Length: 40 })
                return false;

            SHA1 sha = new SHA1CryptoServiceProvider();
            // This is one implementation of the abstract class SHA1.
            hashValue = sha.ComputeHash(hashValue);

            // Create a new instance of DSACryptoServiceProvider.
            using var dsa = new DSACryptoServiceProvider();
            // Import the key information.
            dsa.ImportParameters(dsaKeyInfo);

            // Create an DSASignatureDeformatter object and pass it the
            // DSACryptoServiceProvider to transfer the private key.
            var dsaDeformatter = new DSASignatureDeformatter(dsa);
                
            // Set the hash algorithm to the passed value.
            dsaDeformatter.SetHashAlgorithm(HashAlg);

            // Verify signature and return the result.
            return dsaDeformatter.VerifySignature(hashValue, signedHashValue);
        }
    }
#endif
}