using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DNS.Protocol.Utils;

namespace DNS.Protocol {
    public class Domain : IComparable<Domain> {
        private byte[][] labels;

        public static Domain FromString(string domain) {
            return new Domain(domain);
        }

        public static Domain FromArray(byte[] message, int offset) {
            return FromArray(message, offset, out offset);
        }

        public static Domain FromArray(byte[] message, int offset, out int endOffset) {
            IList<byte[]> labels = new List<byte[]>();
            bool endOffsetAssigned = false;
            endOffset = 0;
            byte lengthOrPointer;

            while ((lengthOrPointer = message[offset++]) > 0) {
                // Two heighest bits are set (pointer)
                if (lengthOrPointer.GetBitValueAt(6, 2) == 3) {
                    if (!endOffsetAssigned) {
                        endOffsetAssigned = true;
                        endOffset = offset + 1;
                    }

                    ushort pointer = lengthOrPointer.GetBitValueAt(0, 6);
                    offset = (pointer << 8) | message[offset];

                    continue;
                } else if (lengthOrPointer.GetBitValueAt(6, 2) != 0) {
                    throw new ArgumentException("Unexpected bit pattern in label length");
                }

                byte length = lengthOrPointer;
                byte[] label = new byte[length];
                Array.Copy(message, offset, label, 0, length);

                labels.Add(label);

                offset += length;
            }

            if (!endOffsetAssigned) {
                endOffset = offset;
            }

            return new Domain(labels.ToArray());
        }

        public static Domain PointerName(IPAddress ip) {
            return new Domain(FormatReverseIP(ip));
        }

        private static string FormatReverseIP(IPAddress ip) {
            byte[] address = ip.GetAddressBytes();

            if (address.Length == 4) {
                return string.Join(".", address.Reverse().Select(b => b.ToString())) + ".in-addr.arpa";
            }

            byte[] nibbles = new byte[address.Length * 2];

            for (int i = 0, j = 0; i < address.Length; i++, j = 2 * i) {
                byte b = address[i];

                nibbles[j] = b.GetBitValueAt(4, 4);
                nibbles[j + 1] = b.GetBitValueAt(0, 4);
            }

            return string.Join(".", nibbles.Reverse().Select(b => b.ToString("x"))) + ".ip6.arpa";
        }

        public Domain(string domain) : this(domain.Split('.')) {}

        public Domain(string[] labels) {
            this.labels = labels.Select(label => Encoding.ASCII.GetBytes(label)).ToArray();
        }

        public Domain(byte[][] labels) {
            this.labels = labels;
        }

        public int Size {
            get { return labels.Sum(l => l.Length) + labels.Length + 1; }
        }

        public byte[] ToArray() {
            byte[] result = new byte[Size];
            int offset = 0;

            foreach (byte[] label in labels) {
                result[offset++] = (byte)label.Length;
                label.CopyTo(result, offset);

                offset += label.Length;
            }

            result[offset] = 0;

            return result;
        }

        public override string ToString() {
            return string.Join(".", labels.Select(label => Encoding.ASCII.GetString(label)));
        }

        public int CompareTo(Domain other) {
            return ToString().ToLower().CompareTo(other.ToString().ToLower());
        }

        public override bool Equals(object obj) {
            if (obj == null) {
                return false;
            }
            if (!(obj is Domain)) {
                return false;
            }

            return CompareTo(obj as Domain) == 0;
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }
    }
}
