using System;
using System.IO;
using HyperLogLog.net.Serializer;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var rnd = new Random();
            var hashAlgorithm = Murmur.MurmurHash.Create128(managed: false);
            var hyperLogLogSerializer = new HyperLogLogSerializer();

            for (int b = 4; b <= 16; b++)
            {
                var hll1 = new HyperLogLog.net.HyperLogLog(hashAlgorithm, b);
                var hll2 = new HyperLogLog.net.HyperLogLog(hashAlgorithm, b);
                var hll3 = new HyperLogLog.net.HyperLogLog(hashAlgorithm, b);

                const int count = 1000000;
                for (int i = 1; i < count + 1; i++)
                {
                    hll1.LogData(Guid.NewGuid());
                    hll2.LogData(Guid.NewGuid());
                    hll3.LogData(Guid.NewGuid());
                }

                var hll = HyperLogLog.net.HyperLogLog.Merge(hll1, hll2, hll3);

                var outputStream = new MemoryStream();
                hyperLogLogSerializer.SerializeTo(hll, outputStream);

                var inputStream = new MemoryStream(outputStream.ToArray());
                var hllDeserialized = hyperLogLogSerializer.DeserializeTo(inputStream, hashAlgorithm);

                var cardinality = hll.GetCount();
                var cardinalityDeserialized = hllDeserialized.GetCount();

                if (cardinality != cardinalityDeserialized)
                    throw new Exception("Serialization/Deserialization ruined the data!");

                var error = 1 - (cardinality/(double) (3 * count));
                Console.WriteLine("B: {0}\tActual: {1}\tEstimated: {2}\t%Error: {3}", b, 3 * count, cardinality, error);
                Console.WriteLine("\t\tEstimated1: {0},{1},{2}", hll1.GetCount(), hll2.GetCount(), hll3.GetCount());
            }
        }
    }
}
