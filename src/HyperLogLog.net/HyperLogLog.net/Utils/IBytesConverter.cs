namespace HyperLogLog.net
{
    public interface IBytesConverter
    {
        byte[] GetBytes(object obj);
    }
}