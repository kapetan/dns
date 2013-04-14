using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNS.Protocol {
    public class Domain {
        private string[] labels;

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

            return new Domain(labels.Select(l => Encoding.ASCII.GetString(l)).ToArray());
        }

        /*public static Domain FromArray2(byte[] message, int offset, out int endOffset) {
            IList<ILabel> labels = new List<ILabel>();
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

            //return new Domain(labels.Select(l => Encoding.ASCII.GetString(l)).ToArray());
        }

        private static IList<ILabel> GetLabels(byte[] message, int offset, out int endOffset) {
            IList<ILabel> labels = new List<ILabel>();
            byte length;

            while((length = message[offset++]) != 0 && length.GetBitValueAt(6, 2) == 0) {
                byte[] label = new byte[length];
                Array.Copy(message, offset, label, 0, length);

                labels.Add(new Label(Encoding.ASCII.GetString(label)));

                offset += length;
            }

            endOffset = offset - 1;
            return labels;
        }*/

        public Domain(string domain) : this(domain.Split('.')) {}

        public Domain(string[] labels) {
            this.labels = labels;
        }

        public int Size {
            get { return labels.Sum(l => l.Length) + labels.Length + 1; }
        }

        /*public void CopyTo(byte[] buffer, int offset = 0) {
            
        }*/

        public byte[] ToArray() {
            byte[] result = new byte[Size];
            int offset = 0;

            foreach (string label in labels) {
                byte[] l = Encoding.ASCII.GetBytes(label);

                result[offset++] = (byte) l.Length;
                l.CopyTo(result, offset);

                offset += l.Length;
            }

            result[offset] = 0;

            return result;
        }

        public override string ToString() {
            return string.Join(".", labels);
        }

        /*private struct Label {
            private const int POINTER_SIZE = 2;

            private string label;
            private ushort? pointer;

            public Label(string label, ushort? pointer = null) {
                this.label = label;
                this.pointer = pointer;
            }

            public int Size {
                get { return pointer != null ? POINTER_SIZE : (label.Length + 1); }
            }

            public void CopyTo(byte[] buffer, int offset) {
                if (pointer != null) {
                    // Get highest byte and set the highest two bits
                    buffer[offset] = ((byte) (pointer >> 8)).SetBitValueAt(6, 2, 3);
                    buffer[offset + 1] = (byte) (pointer & 0xff);
                } else {
                    buffer[offset] = (byte) label.Length;
                    Encoding.ASCII.GetBytes(label).CopyTo(buffer, offset + 1);
                }
            }

            public override string ToString() {
                return label;
            }
        }*/

        private interface ILabel {
            int Size { get; }
            void CopyTo(byte[] buffer, int offset);
        }

        private class Label : ILabel {
            private string value;

            public Label(string value) {
                this.value = value;
            }

            public int Size {
                get { return value.Length + 1; }
            }

            public void CopyTo(byte[] buffer, int offset) {
                buffer[offset] = (byte) value.Length;
                Encoding.ASCII.GetBytes(value).CopyTo(buffer, offset + 1);
            }

            public override string ToString() {
                return value;
            }
        }

        private class Pointer : ILabel {
            private ushort pointer;
            private IList<ILabel> labels;

            public Pointer(ushort pointer, IList<ILabel> labels) {
                this.pointer = pointer;
                this.labels = labels;
            }

            public int Size {
                get { return 2; }
            }

            public void CopyTo(byte[] buffer, int offset) {
                // Get highest byte and set the highest two bits
                buffer[offset] = ((byte) (pointer >> 8)).SetBitValueAt(6, 2, 3);
                buffer[offset + 1] = (byte) (pointer & 0xff);
            }

            public override string ToString() {
                return string.Join(".", labels);
            }
        }
    }
}
