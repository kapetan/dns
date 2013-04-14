using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace DNS.Marshalling {
    public enum Endianness { 
        Big,
        Little,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Struct)]
    public class EndianAttribute : Attribute {
        public EndianAttribute(Endianness endianness) {
            this.Endianness = endianness;
        }

        public Endianness Endianness {
            get;
            private set;
        }
    }

    public class ByteStream : System.IO.Stream {
        private byte[] buffer;
        private int offset = 0;

        public ByteStream(int capacity) { 
            buffer = new byte[capacity];
        }

        public ByteStream Append(IEnumerable<byte[]> buffers) {
            foreach (byte[] buf in buffers) {
                Write(buf, 0, buf.Length);
            }

            return this;
        }

        public ByteStream Append(byte[] buf) {
            Write(buf, 0, buf.Length);
            return this;
        }

        public byte[] ToArray() {
            return buffer;
        }

        public void Reset() {
            this.offset = 0;
        }

        public override bool CanRead {
            get { return false; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return buffer.Length > 0 && offset < buffer.Length; }
        }

        public override void Flush() {}

        public override long Length {
            get { return offset; }
        }

        public override long Position {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override int Read(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin) {
            throw new NotImplementedException();
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            Array.Copy(buffer, offset, this.buffer, this.offset, count);
            this.offset += count;
        }
    }

    public class Object {
        public static Object New(object obj) {
            return new Object(obj);
        }

        public static string Dump(object obj) {
            return DumpObject(obj);
        }

        private static string DumpObject(object obj) {
            if(obj is string) {
                return (string) obj;
            } else if (obj is IDictionary) {
                return DumpDictionary((IDictionary) obj);
            } else if (obj is IEnumerable) {
                return DumpList((IEnumerable) obj);
            } else {
                return obj == null ? "null" : obj.ToString();
            }
        }

        private static string DumpList(IEnumerable enumerable) {
            return "[" + string.Join(", ", enumerable.Cast<object>().Select(o => DumpObject(o)).ToArray()) + "]";
        }

        private static string DumpDictionary(IDictionary dict) {
            StringBuilder result = new StringBuilder();

            result.Append("{");

            foreach (DictionaryEntry pair in dict) {
                result
                    .Append(pair.Key)
                    .Append("=")
                    .Append(DumpObject(pair.Value))
                    .Append(", ");
            }

            if (result.Length > 1) {
                result.Remove(result.Length - 2, 2);
            }

            return result.Append("}").ToString();
        }

        private object obj;
        private Dictionary<string, string> pairs;

        public Object(object obj) {
            this.obj = obj;
            this.pairs = new Dictionary<string, string>();
        }

        public Object Remove(params string[] names) {
            foreach (string name in names) {
                pairs.Remove(name);
            }

            return this;
        }

        public Object Add(params string[] names) {
            Type type = obj.GetType();

            foreach (string name in names) {
                PropertyInfo property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
                object value = property.GetValue(obj, new object[] {});

                pairs.Add(name, DumpObject(value));
            }

            return this;
        }

        public Object Add(string name, object value) {
            pairs.Add(name, DumpObject(value));
            return this;
        }

        public Object AddAll() {
            PropertyInfo[] properties = obj.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties) {
                object value = property.GetValue(obj, new object[] {});
                pairs.Add(property.Name, DumpObject(value));
            }

            return this;
        }

        public override string ToString() {
            return DumpDictionary(pairs);
        }
    }

    public static class Struct {
        private static byte[] ConvertEndian(Type type, byte[] data) {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            EndianAttribute endian = null;

            if (type.IsDefined(typeof(EndianAttribute), false)) {
                endian = (EndianAttribute) type.GetCustomAttributes(typeof(EndianAttribute), false)[0];
            }

            foreach(FieldInfo field in fields) {
                if (endian == null && !field.IsDefined(typeof(EndianAttribute), false)) {
                    continue;
                }

                int offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                int length = Marshal.SizeOf(field.FieldType);
                endian = endian ?? (EndianAttribute) field.GetCustomAttributes(typeof(EndianAttribute), false)[0];

                if (endian.Endianness == Endianness.Big && BitConverter.IsLittleEndian ||
                        endian.Endianness == Endianness.Little && !BitConverter.IsLittleEndian) {
                    Array.Reverse(data, offset, length);
                }
            }

            return data;
        }

        public static T GetStruct<T>(byte[] data) where T : struct {
            return GetStruct<T>(data, 0, data.Length);
        }

        public static T GetStruct<T>(byte[] data, int offset, int length) where T : struct {
            byte[] buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, buffer.Length);

            GCHandle handle = GCHandle.Alloc(ConvertEndian(typeof(T), buffer), GCHandleType.Pinned);

            try {
                return (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            } finally {
                handle.Free();
            }
        }

        public static byte[] GetBytes<T>(T obj) where T : struct {
            byte[] data = new byte[Marshal.SizeOf(obj)];
            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
                return ConvertEndian(typeof(T), data);
            } finally {
                handle.Free();
            }
        }
    }
}
