using System;
using System.Collections;
using System.Text;

namespace HyperLogLog.net
{
    public class BytesConverter : IBytesConverter
    {
        public virtual byte[] GetBytes(object obj)
        {
            if (obj == null)
                return new byte[] { 0 };
            if (obj is byte[])
                return (byte[])obj;
            if (obj is int)
                return BitConverter.GetBytes((int)obj);
            if (obj is uint)
                return BitConverter.GetBytes((uint)obj);
            if (obj is short)
                return BitConverter.GetBytes((short)obj);
            if (obj is ushort)
                return BitConverter.GetBytes((ushort)obj);
            if (obj is bool)
                return BitConverter.GetBytes((bool)obj);
            if (obj is long)
                return BitConverter.GetBytes((long)obj);
            if (obj is ulong)
                return BitConverter.GetBytes((ulong)obj);
            if (obj is char)
                return BitConverter.GetBytes((char)obj);
            if (obj is float)
                return BitConverter.GetBytes((float)obj);
            if (obj is double)
                return BitConverter.GetBytes((double)obj);
            if (obj is decimal)
                return new BitArray(decimal.GetBits((decimal)obj)).ToBytes();
            if (obj is Guid)
                return ((Guid)obj).ToByteArray();
            if (obj is string)
                return Encoding.Unicode.GetBytes((string)obj);

            return Encoding.Unicode.GetBytes(obj.ToString());
        }
    }
}