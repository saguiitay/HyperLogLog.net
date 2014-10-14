using System.IO;
using System.Security.Cryptography;

namespace HyperLogLog.net.Serializer
{
    public class HyperLogLogSerializer
    {
        public void SerializeTo(HyperLogLog hyperLogLog, Stream stream)
        {
            using (var bw = new BinaryWriter(stream))
            {
                bw.Write((byte)hyperLogLog.B);
                bw.Write(hyperLogLog._registers);
                bw.Flush();
            }
        }

        public HyperLogLog DeserializeTo(Stream stream, HashAlgorithm hashAlgorithm)
        {
            return DeserializeTo(stream, hashAlgorithm, new BytesConverter());
        }
        public HyperLogLog DeserializeTo(Stream stream, IBytesConverter bytesConverter)
        {
            return DeserializeTo(stream, MD5.Create(), bytesConverter);
        }

        public HyperLogLog DeserializeTo(Stream stream, HashAlgorithm hashAlgorithm, IBytesConverter bytesConverter)
        {
            using (var br = new BinaryReader(stream))
            {
                var b = br.ReadByte();

                var result = new HyperLogLog(hashAlgorithm, bytesConverter, b);

                for (int i =0; i < result.M; i++)
                {
                    result._registers[i] = br.ReadByte();
                }
                return result;
            }
        }
    }
}
