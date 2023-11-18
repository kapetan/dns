using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Net;
using System.Collections.ObjectModel;

namespace DNS.Protocol.ResourceRecords {
    public class OptResourceRecord : BaseResourceRecord {
        public ushort UdpPayloadSize => (ushort)Class;

        public byte ExtendedResponseCode => (byte)((uint)TimeToLive.TotalSeconds >> 24);

        public byte Version => (byte)((uint)TimeToLive.TotalSeconds >> 16);

        public ushort Z => (ushort)((uint)TimeToLive.TotalSeconds & 0xffff);

        public IReadOnlyList<Option> Options { get; }

        private static IResourceRecord Create(ushort udpPayloadSize, params Option[] options) {
            var data = new byte[(Option.CodeSize + Option.LengthSize) * options.Length + options.Sum(o => o.Length)];
            var offset = 0;

            // https://datatracker.ietf.org/doc/html/rfc6891#section-6.1.2
            // 
            //      0   1   2   3   4   5   6   7   8   9   0   1   2   3   4   5
            //    +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
            // 0: |                          OPTION-CODE                          |
            //    +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
            // 2: |                         OPTION-LENGTH                         |
            //    +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+
            // 4: |                                                               |
            //    /                          OPTION-DATA                          /
            //    /                                                               /
            //    +---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+---+

            foreach (var option in options) {
                data[offset++] = (byte)(option.Code >> 8);
                data[offset++] = (byte)option.Code;

                data[offset++] = (byte)(option.Length >> 8);
                data[offset++] = (byte)option.Length;

                option.Data.CopyTo(data, offset);
                offset += option.Data.Count;
            }

            return new ResourceRecord(Domain.Empty, data, RecordType.OPT, (RecordClass)udpPayloadSize);
        }

        public OptResourceRecord(IResourceRecord record) : base(record) {
            Options = GetOptions();
        }

        public OptResourceRecord(ushort udpPayloadSize, params Option[] options) : base(Create(udpPayloadSize, options)) {
            Options = options;
        }

        private IReadOnlyList<Option> GetOptions() {
            const int OptionHeaderSize = 4;

            if (DataLength == 0)
                return Array.Empty<Option>();
            if (DataLength < OptionHeaderSize)
                throw new InvalidDataException($"The {nameof(Data)} length is less than {OptionHeaderSize}, it is not a valid opt resource record option");

            var options = new List<Option>();
            var offset = 0;

            while (offset < DataLength) {
                var optionCode = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(Data, offset));
                offset += sizeof(ushort);

                var optionLength = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(Data, offset));
                offset += sizeof(ushort);

                var data = new ArraySegment<byte>(Data, offset, optionLength);
                options.Add(new Option(optionCode, data));
                offset += optionLength;

                var remainBytes = DataLength - offset;
                if (remainBytes > 0 && remainBytes < OptionHeaderSize)
                    throw new InvalidDataException($"The remaining bytes of {nameof(Data)} at offset {offset} less than {OptionHeaderSize}, which is not enough to parse as a valid option");
            }

            return options;
        }
    }

    public class Option {
        internal const int CodeSize = 2;
        internal const int LengthSize = 2;

        public ushort Code { get; }

        public ushort Length => (ushort)Data.Count;

        public ReadOnlyCollection<byte> Data { get; }

        public Option(ushort code, IEnumerable<byte> data) {
            Code = code;
            Data = Array.AsReadOnly(data.ToArray());

            if (Data.Count > ushort.MaxValue)
                throw new InvalidOperationException($"Option data length MUST be <= {ushort.MaxValue}.");
        }

        internal Option(ushort code, ArraySegment<byte> data) {
            Code = code;
            Data = new ReadOnlyCollection<byte>(data);
        }
    }
}
