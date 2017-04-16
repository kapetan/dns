using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    
    public class SerializeDomainTest {
        [Fact]
        public void EmptyDomain() {
            Domain domain = new Domain(Helper.GetArray<string>());
            byte[] content = Helper.ReadFixture("Domain", "empty-label");

            Assert.Equal(content, domain.ToArray());
        }

        [Fact]
        public void DomainWithSingleLabel() {
            Domain domain = new Domain(Helper.GetArray("www"));
            byte[] content = Helper.ReadFixture("Domain", "www-label");

            Assert.Equal(content, domain.ToArray());
        }

        [Fact]
        public void DomainWithMultipleLabels() {
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");

            Assert.Equal(content, domain.ToArray());
        }
    }
}
