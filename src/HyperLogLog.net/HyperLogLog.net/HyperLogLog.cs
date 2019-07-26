using System;
using System.Linq;
using System.Security.Cryptography;

namespace HyperLogLog.net
{
    public class HyperLogLog
    {
        internal readonly byte[] _registers;
        private readonly HashAlgorithm _hashAlgorithm;
        private readonly IBytesConverter _bytesConverter;
        private readonly object _hashLock = new object();

        #region Constants

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// 2^32 
        /// </summary>
        public const double Pow2_32 = 4294967296;

        #endregion

        public int B { get; private set; }
        public int M { get; private set; }
        public double Alpha { get; private set; }




        public HyperLogLog(int b = 10)
            : this(MD5.Create(), new BytesConverter(), b)
        { }

        public HyperLogLog(HashAlgorithm hashAlgorithm, int b = 10)
            : this(hashAlgorithm, new BytesConverter(), b)
        { }

        public HyperLogLog(IBytesConverter bytesConverter, int b = 10)
            : this(MD5.Create(), bytesConverter, b)
        { }

        public HyperLogLog(HashAlgorithm hashAlgorithm, IBytesConverter bytesConverter, int b = 10)
        {
            if (b < 4 || b > 16)
                throw new ArgumentOutOfRangeException("b", string.Format("HyperLogLog accuracy of {0} is not supported. Please use a B value between 4 and 16.", b));

            _hashAlgorithm = hashAlgorithm;
            _bytesConverter = bytesConverter;
            B = b;
            M = (int)Math.Pow(2, b);

            Alpha = ComputeAlpha();
            _registers = new byte[M];


        }


        public void LogData(object data)
        {
            var hash = GetHash(data);

           LogHash(hash);
        }

        public void LogHash(ulong hash)
        {
            var registerIndex = GetRegisterIndex(hash); // binary address of the rightmost b bits
            var runLength = RunOfZeros(hash); // length of the run of zeroes starting at bit b+1
            _registers[registerIndex] = Math.Max(_registers[registerIndex], runLength);
        }
        
        
        
        public int GetCount()
        {
            int dv;

            var registersSum = _registers.Sum(r => 1d / FastPow2(r));
            var dvEstimate = Alpha*((double)M*M) / registersSum;

            if (dvEstimate < 2.5 * M)
            {
                var v = _registers.Count(r => r == 0);
                if (v == 0)
                    dv = (int)dvEstimate;
                else
                    dv = (int)(M * Math.Log((double)M / v));
            }
            else if (dvEstimate <= (Pow2_32 * 1 / 30))  // 143,165,576
                dv = (int)dvEstimate;
            else
            {
                if (dvEstimate <= 1E9)
                    dv = (int)(-Pow2_32 * Math.Log(1 - dvEstimate / Pow2_32));
                else
                    throw new ArgumentException("Estimated cardinality exceeds 10^9");
            }

            return dv;
        }

        private double FastPow2(int x)
        {
            return (ulong)1 << x;  // right up to 2^63
        }

        private ulong GetHash(object data)
        {
            var dataBytes = _bytesConverter.GetBytes(data);
            var hashBytes = new byte[0];

            lock (_hashLock)
            {
                hashBytes = _hashAlgorithm.ComputeHash(dataBytes);
            }

            // XORing the hash into 64bits sections is not required - we can just take the first 64 bits
            //ulong hash = 0;
            //for (int x = 0; x < dataBytes.Length; x += 8)
            //{
            //    hash ^= BitConverter.ToUInt64(hashBytes, x);
            //}
            //return hash;

            return BitConverter.ToUInt64(hashBytes, 0);
        }

        private uint GetRegisterIndex(ulong hash)
        {
            var index = hash & ((uint)Math.Pow(2, B) - 1);
            return (uint)index;
        }

        private byte RunOfZeros(ulong hash)
        {
            var value = hash >> B;

            var shifted = 0;
            byte count = 1;
            while (((value & 1) == 0) && shifted < 64)
            {
                value >>= 1;
                count++;
                shifted++;
            }
            return count;
        }

        

        private double ComputeAlpha()
        {
            if (M == 16)
                return 0.673;
            if (M == 32)
                return 0.697;
            if (M == 64)
                return 0.709;
            return 0.7213/(1 + (1.079/M));
        }


        public static HyperLogLog Merge(params HyperLogLog[] hlls)
        {
            if (hlls == null || hlls.Length == 0)
                return new HyperLogLog();

            var b = hlls[0].B;

            if (hlls.Any(hyperLogLog => b != hyperLogLog.B))
                throw new ArgumentException("All HyperLogLogs needs to be on the same size", "hlls");


            var result = new HyperLogLog(hlls[0]._hashAlgorithm, hlls[0]._bytesConverter, hlls[0].B);

            for (int i = 0; i < result.M; i++)
            {
                result._registers[i] = hlls.Max(x => x._registers[i]);
            }

            return result;
        }
    }
}
