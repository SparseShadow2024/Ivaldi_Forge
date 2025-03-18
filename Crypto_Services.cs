using System.Text;
using Extensions.Data;

namespace Ivaldi
{
    public class Crypto_Services
    {
        public static byte[] XOR(byte[] data, byte[] key)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }
        public static byte[] Advanced_XOR(byte[] Value, byte[] Key)
        {
            if (Value.Length == Key.Length)
            {
                return XOR(Value, Key);
            }
            else if (Value.Length < Key.Length)
            {
                byte[] trimmed_key_bytes = new byte[Value.Length];
                Array.Copy(Key, trimmed_key_bytes, Value.Length);
                return XOR(Value, trimmed_key_bytes);
            }
            else
            {
                List<byte> final_bytes_list = new List<byte>();
                for (int i = 0; i <= Value.Length - Key.Length; i += Key.Length)
                {
                    byte[] trimmed_key_bytes = new byte[Key.Length];
                    Array.Copy(Value, i, trimmed_key_bytes, 0, Key.Length);
                    final_bytes_list.AddRange(XOR(trimmed_key_bytes, Key));
                }
                int remain_bytes_length = Value.Length % Key.Length;
                if (remain_bytes_length > 0)
                {
                    byte[] remain_bytes = new byte[remain_bytes_length];
                    Array.Copy(Value, Value.Length - remain_bytes_length, remain_bytes, 0, remain_bytes_length);

                    byte[] trimmed_key_bytes = new byte[remain_bytes_length];
                    Array.Copy(Key, trimmed_key_bytes, remain_bytes_length);

                    final_bytes_list.AddRange(XOR(remain_bytes, trimmed_key_bytes));
                }

                return final_bytes_list.ToArray();
            }
        }
        public static byte[] Generate_Password(string Key, int Password_Lenth)
        {
            uint seed = XXHash.XXH32(Encoding.UTF8.GetBytes(Key), 0);
            MX.MersenneTwister mersenne_twister = new MX.MersenneTwister((int)seed);
            return mersenne_twister.NextBytes(Password_Lenth);
        }
        public static byte[] Generate_Password_Old(string key, int Type)
        {
            int Needed_Bytes = 0;
            switch (Type)
            {
                case 0:
                    Needed_Bytes = 8;
                    break;
                case 1:
                    Needed_Bytes = 15;
                    break;
            }
            byte[] Password = GC.AllocateUninitializedArray<byte>(Needed_Bytes);
            var hash = XXHash.XXH32(Encoding.UTF8.GetBytes(key), 0);
            var mt = new MathNet.Numerics.Random.MersenneTwister((int)hash);
            int i = 0;
            while (i < Password.Length)
            {
                Array.Copy(BitConverter.GetBytes(mt.Next()), 0, Password, i, Math.Min(4, Password.Length - i));
                i += 4;
            }
            return Password;
        }
    }
}

namespace MX
{
    public class MersenneTwister
    {
        private const int N = 624;
        private const int M = 397;
        private const uint MATRIX_A = 0x9908B0DF;
        private const uint UPPER_MASK = 0x80000000;
        private const uint LOWER_MASK = 0x7FFFFFFF;
        private uint[] mag01 = { 0x0, MATRIX_A };
        private uint[] mt = new uint[N];
        private int mti = N + 1;
        public MersenneTwister(int? seed = null)
        {
            seed ??= (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            init_genrand((uint)seed);
        }
        public int Next(int minValue = 0, int? maxValue = null)
        {
            if (maxValue == null)
            {
                return genrand_int31();
            }

            if (minValue > maxValue)
            {
                (minValue, maxValue) = (maxValue.Value, minValue);
            }

            return (int)Math.Floor((maxValue.Value - minValue + 1) * genrand_real1() + minValue);
        }
        public byte[] NextBytes(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i += 4)
            {
                uint randInt = (uint)genrand_int31();
                byte[] intBytes = BitConverter.GetBytes(randInt);
                Array.Copy(intBytes, 0, bytes, i, Math.Min(4, length - i));
            }
            return bytes;
        }
        private void init_genrand(uint s)
        {
            mt[0] = s;
            for (int i = 1; i < N; i++)
            {
                mt[i] = (uint)(1812433253 * (mt[i - 1] ^ (mt[i - 1] >> 30)) + i);
            }
            mti = N;
        }
        private uint genrand_int32()
        {
            uint y;
            if (mti >= N)
            {
                if (mti == N + 1)
                {
                    init_genrand(5489);
                }
                for (int kk = 0; kk < N - M; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                for (int kk = N - M; kk < N - 1; kk++)
                {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1];
                mti = 0;
            }
            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9D2C5680;
            y ^= (y << 15) & 0xEFC60000;
            y ^= (y >> 18);
            return y;
        }
        private int genrand_int31()
        {
            return (int)(genrand_int32() >> 1);
        }
        private double genrand_real1()
        {
            return genrand_int32() * (1.0 / 4294967295.0);
        }
    }
}