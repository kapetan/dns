using Kapetan.Dns.Model;

namespace Kapetan.Dns.Interface
{
    public interface IMessageEntry
    {
        Domain Name { get; }
        RecordType Type { get; }
        RecordClass Class { get; }

        int Size { get; }
        byte[] ToArray();
    }
}
