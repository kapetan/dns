using Kapetan.Dns.Model;

namespace Kapetan.Dns.Interface
{
    public interface IRequest : IMessage
    {
        int Id { get; set; }
        OperationCode OperationCode { get; set; }
        bool RecursionDesired { get; set; }
    }
}
