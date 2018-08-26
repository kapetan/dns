﻿using System.Collections.Generic;
using System.Net;
using DNS.Protocol.ResourceRecords;

namespace DNS.Protocol {
    public interface IRequest : IMessage {
        int Id { get; set; }
        IList<IResourceRecord> AdditionalRecords { get; }
        OperationCode OperationCode { get; set; }
        bool RecursionDesired { get; set; }
        IPAddress RemoteAddress { get; set; }
    }
}
