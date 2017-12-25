using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {

    public class SerializeCharacterStringTest {
        [Fact]
        public void EmptyCharacterString() {
            CharacterString characterString = new CharacterString(new byte[0]);
            byte[] content = Helper.ReadFixture("CharacterString", "empty-string");

            Assert.Equal(content, characterString.ToArray());
            Assert.Equal("", characterString.ToString());
        }

        [Fact]
        public void SimpleCharacterString() {
            CharacterString characterString = new CharacterString("www");
            byte[] content = Helper.ReadFixture("CharacterString", "www-string");

            Assert.Equal(content, characterString.ToArray());
            Assert.Equal("www", characterString.ToString());
        }
    }
}
