using System;

namespace Kapetan.Dns.Interface
{
    public interface IResourceRecord : IMessageEntry
    {
        TimeSpan TimeToLive { get; }
        int DataLength { get; }
        byte[] Data { get; }
    }
}
