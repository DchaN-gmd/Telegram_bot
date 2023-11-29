using System;
using System.Collections.Generic;

namespace DevXUnity.SerialNumberLicense.Tools
{
    public sealed class SerialNumberVerifySimple
    {
        #region Readonly fields
        private readonly MD4 _hashMD4 = new();

        // key pair len * _signLen
        private readonly int _keyLen = 10000;
        
        // output key len
        private readonly int _signLen = 8;
        private readonly byte[] _publicKey;
        #endregion
        
        private byte[] _secretKey;

        public SerialNumberVerifySimple(string serializedKey)
        {
            if (string.IsNullOrEmpty(serializedKey)) return;

            var i = 0;
            var l = serializedKey.Replace("\r\n", "\n").Trim().Split('\n');

            _signLen = int.Parse(l[i]); 
            i++;
            _secretKey = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]); 
            i++;
            _publicKey = string.IsNullOrEmpty(l[i]) ? null : Convert.FromBase64String(l[i]);

            _keyLen = _publicKey.Length / _signLen;
        }
        
        public bool VerifySignature(byte[] value, byte[] signature)
        {
            if (value == null || value.Length == 0 || signature == null || 
                signature.Length == 0 || signature.Length != _signLen || _publicKey == null)
                return false;

            var hash = MakeHash(value);
            var k = BitConverter.ToUInt32(hash, hash.Length - _signLen);

            k %= (uint)_keyLen;
            var step = k * _signLen;

            var standard = new byte[_signLen];
            for (var i = 0; i < _signLen; i++)
                standard[i] = _publicKey[step + i];

            var signatureHash = MakeHash(signature);
            for (var i = 0; i < _signLen; i++)
                if (standard[i] != signatureHash[i])
                    return false;
            
            return true;
        }
        
        private byte[] MakeHash(byte[] value)
        {
            _hashMD4.Initialize();
            var value2 = _hashMD4.ComputeHash(value);

            // For slow only
            for (var i = 0; i < 100; i++)
            {
                value[0] = (byte)i;
                _hashMD4.Initialize();
                value = _hashMD4.ComputeHash(value);
            }

            for(var i = 0; i<value.Length; i++)
                value2[i] = (byte)( value[i] ^ (value2[i]+i));
            
            return value2;
        }
        
        private sealed class MD4
        {
            #region Consts
            private const int BitsInUint = 32;
            private const int BitsInByte = 8;
            private const int BytesInUint = 4;

            private const int HashSize = 16;
            private const int ContextSize = HashSize / BytesInUint;
            private const int CountSize = 8;
            private const int BlockSize = 64;
            private const int LastBITMask = 0x80;
            private const int FinalScope = 56;

            private const int UintBlockSize = BlockSize / BytesInUint;

            private const uint I0 = 0x67452301;
            private const uint I1 = 0xEFCDAB89;
            private const uint I2 = 0x98BADCFE;
            private const uint I3 = 0x10325476;

            private const uint C2 = 0x5A827999;
            private const uint C3 = 0x6ED9EBA1;

            private const int Fs1 = 3;
            private const int Fs2 = 7;
            private const int Fs3 = 11;
            private const int Fs4 = 19;

            private const int Gs1 = 3;
            private const int Gs2 = 5;
            private const int Gs3 = 9;
            private const int Gs4 = 13;

            private const int Hs1 = 3;
            private const int Hs2 = 9;
            private const int Hs3 = 11;
            private const int Hs4 = 15;
            #endregion

            private readonly byte[] _count;
            
            private uint[] _context;
            
            public MD4()
            {
                _context = new uint[ContextSize];
                _count = new byte[CountSize];

                InitContext();
            }
            
            public void Initialize()
            {
                InitContext();
                Array.Clear(_count, 0, CountSize);
            }

            /// <summary>
            /// Computes the hash value for the specified byte array.
            /// </summary>
            /// <param name="array">The input to compute the hash code for.</param>
            /// <returns>The computed hash code.</returns>
            public byte[] ComputeHash(byte[] array) =>
                ComputeHash(array, 0, array.Length);

            /// <summary>
            /// Computes the hash value for the specified region of the specified byte array.
            /// </summary>
            /// <param name="array">The input to compute the hash code for.</param>
            /// <param name="offset">The offset into the byte array from which to begin using data.</param>
            /// <param name="count">The number of bytes in the array to use as data.</param>
            /// <returns>The computed hash code.</returns>
            /// <exception cref="ArgumentNullException">array is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">array is empty or invalid count.</exception>
            private byte[] ComputeHash(byte[] array, int offset, int count)
            {
                var temp = ComputeUInt32Hash(array, offset, count);
                var result = new byte[HashSize];
                Buffer.BlockCopy(temp, 0, result, 0, HashSize);
                return result;
            }

            /// <summary>
            /// Computes the hash value for the specified region of the specified byte array.
            /// </summary>
            /// <param name="array">The input to compute the hash code for.</param>
            /// <param name="offset">The offset into the byte array from which to begin using data.</param>
            /// <param name="count">The number of bytes in the array to use as data.</param>
            /// <returns>The computed hash code.</returns>
            /// <exception cref="ArgumentNullException">array is null.</exception>
            /// <exception cref="ArgumentOutOfRangeException">array is empty or invalid count.</exception>
            private uint[] ComputeUInt32Hash(byte[] array, int offset, int count)
            {
                if (null == array) return null;
                if (0 == array.Length) return null;

                var length = offset + count;
                if (array.Length < length) length = array.Length;

                var block = new uint[UintBlockSize];
                var pos = offset;

                for (; pos <= length - BlockSize; pos += BlockSize)
                {
                    Buffer.BlockCopy(array, pos, block, 0, BlockSize);
                    TransformBlock(block);
                }

                var bitsCount = (ulong)length * BitsInByte;
                BitConverter.GetBytes(bitsCount).CopyTo(_count, 0);

                Array.Clear(block, 0, UintBlockSize);
                Buffer.BlockCopy(array, pos, block, 0, length - pos);
                TransformFinalBlock(block, length - pos);

                return _context;
            }

            private void InitContext()
            {
                _context = new uint[ContextSize];

                _context[0] = I0;
                _context[1] = I1;
                _context[2] = I2;
                _context[3] = I3;
            }

            private void TransformFinalBlock(uint[] block, int len)
            {
                Buffer.BlockCopy(new byte[] { LastBITMask }, 0, block, len, 1);

                if (len < FinalScope)
                {
                    Buffer.BlockCopy(_count, 0, block, FinalScope, CountSize);
                    TransformBlock(block);
                }
                else
                {
                    TransformBlock(block);
                    Array.Clear(block, 0, UintBlockSize);
                    Buffer.BlockCopy(_count, 0, block, FinalScope, CountSize);
                    TransformBlock(block);
                }
            }

            private void TransformBlock(IReadOnlyList<uint> value)
            {
                if (value.Count != UintBlockSize) return;

                var a = _context[0];
                var b = _context[1];
                var c = _context[2];
                var d = _context[3];

                a = Ff(a, b, c, d, value[0], Fs1);
                d = Ff(d, a, b, c, value[1], Fs2);
                c = Ff(c, d, a, b, value[2], Fs3);
                b = Ff(b, c, d, a, value[3], Fs4);
                a = Ff(a, b, c, d, value[4], Fs1);
                d = Ff(d, a, b, c, value[5], Fs2);
                c = Ff(c, d, a, b, value[6], Fs3);
                b = Ff(b, c, d, a, value[7], Fs4);
                a = Ff(a, b, c, d, value[8], Fs1);
                d = Ff(d, a, b, c, value[9], Fs2);
                c = Ff(c, d, a, b, value[10], Fs3);
                b = Ff(b, c, d, a, value[11], Fs4);
                a = Ff(a, b, c, d, value[12], Fs1);
                d = Ff(d, a, b, c, value[13], Fs2);
                c = Ff(c, d, a, b, value[14], Fs3);
                b = Ff(b, c, d, a, value[15], Fs4);

                a = Gg(a, b, c, d, value[0], Gs1);
                d = Gg(d, a, b, c, value[4], Gs2);
                c = Gg(c, d, a, b, value[8], Gs3);
                b = Gg(b, c, d, a, value[12], Gs4);
                a = Gg(a, b, c, d, value[1], Gs1);
                d = Gg(d, a, b, c, value[5], Gs2);
                c = Gg(c, d, a, b, value[9], Gs3);
                b = Gg(b, c, d, a, value[13], Gs4);
                a = Gg(a, b, c, d, value[2], Gs1);
                d = Gg(d, a, b, c, value[6], Gs2);
                c = Gg(c, d, a, b, value[10], Gs3);
                b = Gg(b, c, d, a, value[14], Gs4);
                a = Gg(a, b, c, d, value[3], Gs1);
                d = Gg(d, a, b, c, value[7], Gs2);
                c = Gg(c, d, a, b, value[11], Gs3);
                b = Gg(b, c, d, a, value[15], Gs4);

                a = Hh(a, b, c, d, value[0], Hs1);
                d = Hh(d, a, b, c, value[8], Hs2);
                c = Hh(c, d, a, b, value[4], Hs3);
                b = Hh(b, c, d, a, value[12], Hs4);
                a = Hh(a, b, c, d, value[2], Hs1);
                d = Hh(d, a, b, c, value[10], Hs2);
                c = Hh(c, d, a, b, value[6], Hs3);
                b = Hh(b, c, d, a, value[14], Hs4);
                a = Hh(a, b, c, d, value[1], Hs1);
                d = Hh(d, a, b, c, value[9], Hs2);
                c = Hh(c, d, a, b, value[5], Hs3);
                b = Hh(b, c, d, a, value[13], Hs4);
                a = Hh(a, b, c, d, value[3], Hs1);
                d = Hh(d, a, b, c, value[11], Hs2);
                c = Hh(c, d, a, b, value[7], Hs3);
                b = Hh(b, c, d, a, value[15], Hs4);

                _context[0] += a;
                _context[1] += b;
                _context[2] += c;
                _context[3] += d;
            }

            private static uint Rot(uint t, int s)
            {
                var result = (t << s) | (t >> (BitsInUint - s));
                return result;
            }

            private static uint F(uint x, uint y, uint z)
            {
                var t = (x & y) | (~x & z);
                return t;
            }

            private static uint G(uint x, uint y, uint z)
            {
                var t = (x & y) | (x & z) | (y & z);
                return t;
            }

            private static uint H(uint x, uint y, uint z)
            {
                var t = x ^ y ^ z;
                return t;
            }

            private static uint Ff(uint a, uint b, uint c, uint d, uint x, int s)
            {
                var t = a + F(b, c, d) + x;
                return Rot(t, s);
            }

            private static uint Gg(uint a, uint b, uint c, uint d, uint x, int s)
            {
                var t = a + G(b, c, d) + x + C2;
                return Rot(t, s);
            }

            private static uint Hh(uint a, uint b, uint c, uint d, uint x, int s)
            {
                var t = a + H(b, c, d) + x + C3;
                return Rot(t, s);
            }
        }
    }
}