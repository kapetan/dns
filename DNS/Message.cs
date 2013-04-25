using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DNS.Protocol {
    public interface IMessage {
        IList<Question> Questions { get; }

        int Size { get; }
        byte[] ToArray(bool lengthPrefix = false);
    }

    public interface IMessageEntry {
        Domain Name { get; }
        RecordType Type { get; }
        RecordClass Class { get; }

        int Size { get; }
        byte[] ToArray();
    }
}
