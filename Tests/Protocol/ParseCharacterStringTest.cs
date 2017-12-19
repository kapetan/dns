using System;
using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol
{

    public class ParseCharacterStringTest {

        [Fact]
        public void BasicCharacterString() {
            var fixture = new byte[] { 0x03, (byte)'f', (byte)'o', (byte)'o' };
            var result = CharacterString.FromArray(fixture, 0, out var endOffset);

            Assert.Equal(3, result.Value.Length);
            Assert.Equal("foo", result.ToString());
            Assert.Equal(4, endOffset);
        }

        [Fact]
        public void WithWrongLengthUnder() {
            var fixture = new byte[] { 0x01, (byte)'f', (byte)'o' };
            var result = CharacterString.FromArray(fixture, 0, out var endOffset);

            Assert.Equal(1, result.Value.Length);
            Assert.Equal("f", result.ToString());
            Assert.Equal(2, endOffset);
        }

        [Fact]
        public void WithWrongLengthOver() {
            var fixture = new byte[] { 0x03, (byte)'f', (byte)'o' };
            Assert.Throws<ArgumentException>(() => CharacterString.FromArray(fixture, 0));
        }

        [Fact]
        public void WithWrongOffset() {
            var fixture = new byte[] { 0x03, (byte)'f', (byte)'o', (byte)'o' };
            Assert.Throws<IndexOutOfRangeException>(() => CharacterString.FromArray(fixture, 4));
        }

        [Fact]
        public void Empty() {
            var fixture = new byte[] {};
            Assert.Throws<ArgumentException>(() => CharacterString.FromArray(fixture, 0));
        }
    }
}
