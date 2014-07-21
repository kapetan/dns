using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kapetan.Dns.Marshalling
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class EndianAttribute : Attribute
    {
        public EndianAttribute(Endianness endianness)
        {
            this.Endianness = endianness;
        }

        public Endianness Endianness
        {
            get;
            private set;
        }
    }
}
