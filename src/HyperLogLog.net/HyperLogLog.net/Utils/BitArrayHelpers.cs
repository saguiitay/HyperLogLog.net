using System;
using System.Collections;

namespace HyperLogLog.net
{
    /// <summary>
    /// Extension method class to make it easier to work with <see cref="BitArray"/> instances
    /// </summary>
    public static class BitArrayHelpers
    {
        /// <summary>
        /// Converts a <see cref="BitArray"/> into an array of <see cref="byte"/>
        /// </summary>
        public static byte[] ToBytes(this BitArray arr)
        {
            if (arr.Count != 8)
            {
                throw new ArgumentException("Not enough bits to make a byte!");
            }
            var bytes = new byte[(arr.Length - 1) / 8 + 1];
            arr.CopyTo(bytes, 0);
            return bytes;
        }
    }
}