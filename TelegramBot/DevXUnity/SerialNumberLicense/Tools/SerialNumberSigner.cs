using System;
using System.Linq;
using System.Text;

namespace DevXUnity.SerialNumberLicense.Editor
{
    /// <summary>
    /// Serial Generator Signer
    /// </summary>
    internal sealed class SerialNumberSigner
    {
        private SerialNumberSignerSimple _simple;
        #if !UNITY_WSA
        private SerialNumberSignerDsa _dsa;
        #endif

        internal SerialNumberSigner(string serializedKey)
        {
            if (string.IsNullOrEmpty(serializedKey))
                throw new ArgumentNullException("serialized_key - not valid");

#if !UNITY_WSA
            if (serializedKey.StartsWith("DSA:"))
            {
                _dsa = new SerialNumberSignerDsa(serializedKey);
            }
            else _simple = new SerialNumberSignerSimple(serializedKey);
#else
                _simple = new SerialNumberSignerSimple(serializedKey);
#endif
        }
        
        private SerialNumberSigner() { }
        
        internal static SerialNumberSigner CreateSimple(int signatureLength = 8, int keyStoreLength = 1000)
        {
            var obj = new SerialNumberSigner
            {
                _simple = new SerialNumberSignerSimple(signatureLength, keyStoreLength)
            };
            return obj;
        }

#if !UNITY_WSA
        internal static SerialNumberSigner CreateDsa()
        {
            var obj = new SerialNumberSigner
            {
                _dsa = new SerialNumberSignerDsa()
            };
            return obj;
        }
#endif

        /// <summary>
        /// Generate open and close keys
        /// </summary>
        internal void GenerateKeys()
        {
#if !UNITY_WSA
            _dsa?.GenerateKeys();
#endif
            _simple?.GenerateKeys();
        }

        #region Sign
        /// <summary>
        /// Make serial number
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal string Sign(string value) =>
            Sign(Encoding.UTF8.GetBytes(value));

        private string Sign(byte[] value)
        {
#if !UNITY_WSA

            if (_dsa != null)
            {
               return BytesConvertToHexString(_dsa.Sign(value));
            }
#endif
            if (_simple != null)
            {
                return BytesConvertToHexString(_simple.Sign(value));
            }
            return null;
        }
        #endregion
        
        internal string SerializeKeys(bool includePrivateKey)
        {
#if !UNITY_WSA
            if (_dsa != null) return _dsa.SerializeKeys(includePrivateKey);
#endif
            return _simple?.SerializeKeys(includePrivateKey);
        }
        
        private static string BytesConvertToHexString(byte[] buff) =>
            (from c in buff let tmp = c select c).
            Aggregate("", (current, c) => current + $"{c:X2}");
    }
}