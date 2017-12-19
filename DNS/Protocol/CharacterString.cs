using System;
using System.Collections.Generic;
using System.Text;

namespace DNS.Protocol {

    /// <summary>
    /// Implementation of the "character-string" non-terminal as defined in
    /// RFC1035 (chapter 3.3):
    ///   "character-string" is a single length octet followed by that number of
    ///    characters. "character-string" is treated as binary information, and
    ///    can be up to 256 characters in length (including the length octet).
    /// </summary>
    public class CharacterString {

        private const int MAX_SIZE = byte.MaxValue;

        public static CharacterString FromArray(byte[] message, int offset) {
            return FromArray(message, offset, out offset);
        }

        public static CharacterString FromArray(byte[] message, int offset, out int endOffset) {
            if (message.Length < 1) {
                throw new ArgumentException("Empty message");
            }
            byte[] value;
            endOffset = 0;
            var len = message[offset++];
            value = new byte[len];
            Buffer.BlockCopy(message, offset, value, 0, len);
            endOffset = offset + len;
            return new CharacterString(value);
        }

        public static IEnumerable<CharacterString> FromString(string message) {
            return FromString(message, Encoding.ASCII);
        }

        public static IEnumerable<CharacterString> FromString(string message, Encoding encoding) {
            var bytes = encoding.GetBytes(message);
            for (var i = 0; i < bytes.Length; i += MAX_SIZE) {
                var len = Math.Min(bytes.Length - i, MAX_SIZE);
                var chunk = new byte[len + 1];
                chunk[0] = (byte)len;
                Buffer.BlockCopy(bytes, i, chunk, 1, len);
                yield return new CharacterString(chunk);
            }
        }

        private CharacterString(byte[] value) => Value = value;

        public byte[] Value { get; }

        public string ToString(Encoding encoding) {
            return encoding.GetString(Value);
        }

        public override string ToString() {
            return ToString(Encoding.ASCII);
        }
    }
}