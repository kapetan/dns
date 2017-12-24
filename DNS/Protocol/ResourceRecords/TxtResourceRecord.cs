using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DNS.Protocol.ResourceRecords {
    public class TxtResourceRecord : BaseResourceRecord {
        /// Regular expression that matches the attribute name/value.
        /// The first unescaped equal sign is the name/value delimiter.
        private static readonly Regex PATTERN_TXT_RECORD = new Regex(@"^([ -~]*?)(?<!`)=([ -~]*)$");

        /// Regular expression that matches unescaped leading/trailing whitespace.
        private static readonly Regex PATTERN_TRIM_NAME = new Regex(@"^\s+|((?<!`)\s)+$");

        /// Regular expression that matches unescaped characters.
        private static readonly Regex PATTERN_ESCAPE = new Regex(@"([`=])");

        /// Regular expression that matches escaped characters.
        private static readonly Regex PATTERN_UNESCAPE = new Regex(@"`([`=\s])");

        private static CharacterString[] ReadTxtData(byte[] message, int dataOffset) {
            var result = new List<CharacterString>();
            while (dataOffset < message.Length) {
                result.Add(CharacterString.FromArray(message, dataOffset, out var endOffset));
                dataOffset = endOffset;
            }
            return result.ToArray();
        }

        private static byte[] CreateTxtData(string attributeName, string attributeValue) {
            var charStrings = CharacterString.FromString($"{Escape(attributeName)}={attributeValue}");
            return charStrings.SelectMany(c => c.ToArray()).ToArray();
        }

        private static string Trim(string value) => PATTERN_TRIM_NAME.Replace(value, string.Empty);
        private static string Escape(string value) => PATTERN_ESCAPE.Replace(value, "`$1");
        private static string Unescape(string value) => PATTERN_UNESCAPE.Replace(value, "$1");

        public TxtResourceRecord(IResourceRecord record, byte[] message, int dataOffset)
            : base(record) {
            RawTxtData = ReadTxtData(message, dataOffset);
            TxtData = String.Join(string.Empty, RawTxtData.Select(x => x.ToString(Encoding.ASCII)));
            Parse();
        }

        public TxtResourceRecord(Domain domain, string attributeName, string attributeValue, TimeSpan ttl = default(TimeSpan))
            : base(new ResourceRecord(domain, CreateTxtData(attributeName, attributeValue), RecordType.TXT, RecordClass.IN, ttl)) {
            AttributeName = attributeName;
            AttributeValue = attributeValue;
        }

        public CharacterString[] RawTxtData { get; }
        public string TxtData { get; }
        public string AttributeName { get; private set; }
        public string AttributeValue { get; private set; }

        public override string ToString() {
            return Stringify().Add("AttributeName", "AttributeValue").ToString();
        }

        private void Parse() {
            var match = PATTERN_TXT_RECORD.Match(TxtData);
            if (match.Success) {
                AttributeName = (match.Groups[1].Length > 0) ? Unescape(Trim(match.Groups[1].ToString())) : null;
                AttributeValue = Unescape(match.Groups[2].ToString());
            } else {
                AttributeName = null;
                AttributeValue = Unescape(TxtData);
            }
        }
    }
}
