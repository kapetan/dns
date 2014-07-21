using System.Collections.Generic;
using Kapetan.Dns.Model;

namespace Kapetan.Dns.Interface
{
    public interface IMessage
    {
        IList<Question> Questions { get; }

        int Size { get; }
        byte[] ToArray();
    }
}
