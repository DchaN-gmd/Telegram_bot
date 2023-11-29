namespace DevXUnity.SerialNumberLicense.Tools
{
    internal sealed class SerialNumberVerify
    {
        private readonly SerialNumberVerifySimple _simple;
#if !UNITY_WSA
        private readonly SerialNumberVerifyDsa _dsa;
#endif
        
        internal SerialNumberVerify(string serializedKey)
        {
            if (string.IsNullOrEmpty(serializedKey)) return;
            
#if UNITY_WSA
            _simple = new SerialNumberVerifySimple(serializedKey);
#else
            if (serializedKey.StartsWith("DSA:")) _dsa = new SerialNumberVerifyDsa(serializedKey);
            else _simple = new SerialNumberVerifySimple(serializedKey);
#endif
        }

        #region Verify signature functions
        internal bool VerifySignature(string value, string signature) =>
            VerifySignature(System.Text.Encoding.UTF8.GetBytes(value), signature);

        private bool VerifySignature(byte[] value, string signature)
        {
#if !UNITY_WSA
            if (_dsa != null) return _dsa.VerifySignature(value, HexStringToBytes(signature));
#endif
            return _simple != null && _simple.VerifySignature(value, HexStringToBytes(signature));
        }
        #endregion

        private static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)  return null;
            
            if(hexString.Contains("-"))
                hexString = hexString.Replace("-", "").Trim();

            if ((hexString.Length & 1) != 0) return null;

            var result = new byte[hexString.Length / 2];

            for (var i = 0; i < hexString.Length; i += 2)
            {
                if (byte.TryParse(hexString.Substring(i, 2),
                        System.Globalization.NumberStyles.HexNumber, 
                        null, out var b) == false) return null;
                result[i / 2] = b;
            }
            
            return result;
        }
    }
}