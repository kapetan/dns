using System;
using System.Text;
using System.Collections.Generic;
using Xunit;
using DNS.Protocol.ResourceRecords;
using DNS.Protocol;

namespace DNS.Tests.Protocol.ResourceRecords {

    public class TextResourceRecordTest {
        [Theory]
        [InlineData("color=blue", "color", "blue")]
        [InlineData("equation=a=4", "equation", "a=4")]
        [InlineData("a`=a=true", "a=a", "true")]
        [InlineData(@"a\`=a=false", @"a\=a", "false")]
        [InlineData(@"`==\=", "=", @"\=")]
        [InlineData(@"string=""Cat""", "string", @"""Cat""")]
        [InlineData("string2=``abc``", "string2", "`abc`")]
        [InlineData("novalue=", "novalue", "")]
        [InlineData("a b=c d", "a b", "c d")]
        [InlineData("abc` =123 ", "abc ", "123 ")]
        public void Rfc1464Examples(string internalForm, string expAttributeName, string expAttributeValue) {
            TextResourceRecord record = new TextResourceRecord(new ArrayTextResourceRecord(internalForm));
            KeyValuePair<string, string> attribute = record.Attribute;
            Assert.Equal(expAttributeName, attribute.Key);
            Assert.Equal(expAttributeValue, attribute.Value);
        }

        [Theory]
        [InlineData("test", "test", null, "test")]
        [InlineData("=test", "=test", null, "test")]
        [InlineData("", "", null, "")]
        public void NegativeExamples(string input, string expTxtData, string expAttributeName, string expAttributeValue) {
            TextResourceRecord record = new TextResourceRecord(new ArrayTextResourceRecord(input));
            KeyValuePair<string, string> attribute = record.Attribute;

            Assert.Equal(expTxtData, record.ToStringTextData());
            Assert.Equal(expAttributeName, attribute.Key);
            Assert.Equal(expAttributeValue, attribute.Value);
        }

        private class ArrayTextResourceRecord : IResourceRecord {
            private static byte[] ToArray(string data) {
                byte[] bytes = Encoding.ASCII.GetBytes(data);
                byte[] result = new byte[bytes.Length + 1];
                result[0] = (byte) bytes.Length;
                Array.Copy(bytes, 0, result, 1, bytes.Length);
                return result;
            }

            public ArrayTextResourceRecord(string data) : this(ToArray(data)) {}

            public ArrayTextResourceRecord(byte[] data) {
                Data = data;
            }

            public TimeSpan TimeToLive {
                get { return TimeSpan.FromMilliseconds(0); }
            }

            public int DataLength {
                get { return Data.Length; }
            }

            public byte[] Data { get; }

            public Domain Name {
                get { return Domain.FromString(""); }
            }

            public RecordType Type {
                get { return RecordType.TXT; }
            }

            public RecordClass Class {
                get { return RecordClass.IN; }
            }

            public int Size {
                get { return Name.Size + Data.Length + 10; }
            }

            public byte[] ToArray() {
                return new byte[Size];
            }
        }
    }
}
