using System;
using System.Security.Cryptography;

namespace DevXUnity.SerialNumberLicense.Editor
{
#if UNITY_WSA
#else
    public sealed class SerialNumberSignerDsa
    {
        private const string HashAlg = "SHA1";

        #region Fields
        private DSAParameters _privateKeyInfo;
        private DSAParameters _publicKeyInfo;
        #endregion

        #region Constructors
        public SerialNumberSignerDsa() { }
        
        public SerialNumberSignerDsa(string serializedKey)
        {
            if (string.IsNullOrEmpty(serializedKey))
                throw new ArgumentNullException("serialized_key - not valid");

            if (serializedKey.StartsWith("DSA:P:"))
                _privateKeyInfo = Parce(serializedKey["DSA:P:".Length..]);

            if (serializedKey.StartsWith("DSA:O:"))
                _publicKeyInfo = Parce(serializedKey["DSA:O:".Length..]);
        }
        #endregion
        
        public void GenerateKeys()
        {
            // Create a new instance of DSACryptoServiceProvider to generate
            // a new key pair.
            using var dsa = new DSACryptoServiceProvider();
            _privateKeyInfo = dsa.ExportParameters(true);
            _publicKeyInfo = dsa.ExportParameters(false);
        }
        
        public byte[] Sign(byte[] value)
        {
            if (value is not { Length: > 0 }) return null;
            var signature = SignHash(value, _privateKeyInfo);
            return signature;
        }

        public string SerializeKeys(bool includePrivateKey)
        {
            if (includePrivateKey) return "DSA:P:" + Serialize(_privateKeyInfo);
            return "DSA:O:" + Serialize(_publicKeyInfo);
        }

        private static string Serialize(DSAParameters val)
        {
            var s = val.Counter + "\n"
                + (val.G == null ? "" : (Convert.ToBase64String(val.G))) + "\n"
                + (val.J == null ? "" : (Convert.ToBase64String(val.J))) + "\n"
                + (val.P == null ? "" : (Convert.ToBase64String(val.P))) + "\n"
                + (val.Q == null ? "" : (Convert.ToBase64String(val.Q))) + "\n"
                + (val.Seed == null ? "" : (Convert.ToBase64String(val.Seed))) + "\n"
                + (val.X == null ? "" : (Convert.ToBase64String(val.X))) + "\n"
                + (val.Y == null ? "" : (Convert.ToBase64String(val.Y)))
                ;
            return s;
        }
        
        static DSAParameters Parce(string s)
        {
            var val = new DSAParameters();
            if (string.IsNullOrEmpty(s))
                return new DSAParameters();

            var l = s.Replace("\r\n", "\n").Split('\n');

            var i = 0;
            val.Counter = int.Parse(l[i]); i++;
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
        
        private static byte[] SignHash(byte[] hashToSign, DSAParameters dsaKeyInfo)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            // This is one implementation of the abstract class SHA1.
            hashToSign = sha.ComputeHash(hashToSign);

            // Create a new instance of DSACryptoServiceProvider.
            using var dsa = new DSACryptoServiceProvider();
            // Import the key information.
            dsa.ImportParameters(dsaKeyInfo);

            // Create an DSASignatureFormatter object and pass it the
            // DSACryptoServiceProvider to transfer the private key.
            var dsaFormatter = new DSASignatureFormatter(dsa);

            // Set the hash algorithm to the passed value.
            dsaFormatter.SetHashAlgorithm(HashAlg);

            // Create a signature for HashValue and return it.
            return dsaFormatter.CreateSignature(hashToSign);
        }
    }
#endif
}