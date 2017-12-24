using System;
using System.Text;
using System.Collections.Generic;
using Xunit;
using DNS.Protocol.ResourceRecords;

namespace DNS.Tests.Protocol.ResourceRecords {

    public class TxtResourceRecordTest {
        [Theory]
        [InlineData(@"color=blue",      @"color",    @"blue")]
        [InlineData(@"equation=a=4",    @"equation", @"a=4")]
        [InlineData(@"a`=a=true",       @"a=a",      @"true")]
        [InlineData(@"a\`=a=false",     @"a\=a",     @"false")]
        [InlineData(@"`==\=",           @"=",        @"\=")]
        [InlineData(@"string=""Cat""",  @"string",   @"""Cat""")]
        [InlineData(@"string2=``abc``", @"string2",  @"`abc`")]
        [InlineData(@"novalue=",        @"novalue",  @"")]
        [InlineData(@"a b=c d",         @"a b",      @"c d")]
        [InlineData(@"abc` =123 ",      @"abc ",     @"123 ")]
        public void Rfc1464Examples(string internalForm, string expAttributeName, string expAttributeValue) {
            TxtResourceRecord record = new TxtResourceRecord(null, Prepare(internalForm), 0);
            KeyValuePair<string, string> attribute = record.Attribute;
            Assert.Equal(expAttributeName, attribute.Key);
            Assert.Equal(expAttributeValue, attribute.Value);
        }

        [Theory]
        [InlineData(@"test",  @"test",  null, @"test")]
        [InlineData(@"=test", @"=test", null, @"test")]
        [InlineData(@"",      @"",      null, @"")]
        public void NegativeExamples(string input, string expTxtData, string expAttributeName, string expAttributeValue) {
            TxtResourceRecord record = new TxtResourceRecord(null, Prepare(input), 0);
            KeyValuePair<string, string> attribute = record.Attribute;

            Assert.Equal(expTxtData, record.ToStringTextData());
            Assert.Equal(expAttributeName, attribute.Key);
            Assert.Equal(expAttributeValue, attribute.Value);
        }

        private byte[] Prepare(string internalForm) {
            var bytes = Encoding.ASCII.GetBytes(internalForm);
            var result = new byte[bytes.Length + 1];
            result[0] = (byte) bytes.Length;
            Array.Copy(bytes, 0, result, 1, bytes.Length);
            return result;
        }
    }
}
