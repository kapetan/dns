using Xunit;
using DNS.Protocol;
using System.Linq;

namespace DNS.Tests.Protocol
{

    public class SerializeCharacterStringTest {

        [Fact]
        public void EmptyString() {
            var result = CharacterString.FromString("");
            Assert.Empty(result);
        }

        [Fact]
        public void SimpleString() {
            var testString = "hello world";
            var testEncoding = System.Text.Encoding.ASCII;
            var result = CharacterString.FromString(testString, testEncoding);
            Assert.Equal(1, result.Count());
            var resultBytes = result.First().Value;
            Assert.Equal(testString.Length + 1, resultBytes.Length);
            Assert.Equal(testString.Length, resultBytes[0]);
            Assert.Equal(testString, testEncoding.GetString(resultBytes, 1, resultBytes.Length - 1));
        }

        [Fact]
        public void LongString() {
            var testString = new string('a', 512);
            var result = CharacterString.FromString(testString);
            Assert.Equal(3, result.Count());
            Assert.Equal(new []{256, 256, 3}, result.Select(x => x.Value.Length));
            Assert.Equal(new []{255, 255, 2}, result.Select(x => (int)x.Value[0]));
        }
    }
}
