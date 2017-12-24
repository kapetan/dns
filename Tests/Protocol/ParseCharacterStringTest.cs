using System;
using Xunit;
using DNS.Protocol;
using System.Linq;
using System.Collections.Generic;

namespace DNS.Tests.Protocol {

    public class ParseCharacterStringTest {
        [Fact]
        public void EmptyCharacterString() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("CharacterString", "empty-string");
            CharacterString characterString = CharacterString.FromArray(content, 0, out endOffset);

            Assert.Equal("", characterString.ToString());
            Assert.Equal(1, characterString.Size);
            Assert.Equal(1, endOffset);
        }

        [Fact]
        public void SimpleCharacterString() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("CharacterString", "www-string");
            CharacterString characterString = CharacterString.FromArray(content, 0, out endOffset);

            Assert.Equal("www", characterString.ToString());
            Assert.Equal(4, characterString.Size);
            Assert.Equal(4, endOffset);
        }

        [Fact]
        public void CharacterStringWithWrongLengthUnder() {
            var fixture = new byte[] { 0x01, (byte)'f', (byte)'o' };
            var result = CharacterString.FromArray(fixture, 0, out var endOffset);

            Assert.Equal(2, result.Size);
            Assert.Equal("f", result.ToString());
            Assert.Equal(2, endOffset);
        }

        [Fact]
        public void CharacterStringWithWrongLengthOver() {
            var fixture = new byte[] { 0x03, (byte)'f', (byte)'o' };
            Assert.Throws<ArgumentException>(() => CharacterString.FromArray(fixture, 0));
        }

        [Fact]
        public void CharacterStringWithWrongOffset() {
            var fixture = new byte[] { 0x03, (byte)'f', (byte)'o', (byte)'o' };
            Assert.Throws<IndexOutOfRangeException>(() => CharacterString.FromArray(fixture, 4));
        }

        [Fact]
        public void CharacterStringWithEmptyData() {
            var fixture = new byte[] {};
            Assert.Throws<ArgumentException>(() => CharacterString.FromArray(fixture, 0));
        }

        [Fact]
        public void MultipleCharacterStrings() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("CharacterString", "www.google.com-string");
            IList<CharacterString> characterStrings = CharacterString.GetAllFromArray(content, 0, out endOffset);

            Assert.Equal(3, characterStrings.Count);
            Assert.Equal(15, endOffset);

            CharacterString characterString = characterStrings[0];

            Assert.Equal("www", characterString.ToString());
            Assert.Equal(4, characterString.Size);

            characterString = characterStrings[1];

            Assert.Equal("google", characterString.ToString());
            Assert.Equal(7, characterString.Size);

            characterString = characterStrings[2];

            Assert.Equal("com", characterString.ToString());
            Assert.Equal(4, characterString.Size);
        }

        [Fact]
        public void CharacterStringFromEmptyString() {
            var result = CharacterString.FromString("");
            Assert.Empty(result);
        }

        [Fact]
        public void CharacterStringFromSimpleString() {
            var result = CharacterString.FromString("www");

            Assert.Equal(1, result.Count);

            var characterString = result.First();

            Assert.Equal("www", characterString.ToString());
            Assert.Equal(4, characterString.Size);
        }

        [Fact]
        public void CharacterStringFromLongString() {
            var result = CharacterString.FromString(new string('a', 512));

            Assert.Equal(3, result.Count);
            Assert.Equal(new []{256, 256, 3}, result.Select(x => x.Size));
            Assert.Equal(new []{255, 255, 2}, result.Select(x => (int) x.ToArray()[0]));
        }
    }
}
