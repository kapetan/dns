using System;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class SerializeDomainTest {
        [Test]
        public void EmptyDomain() {
            Domain domain = new Domain(Helper.GetArray<string>());
            byte[] content = Helper.ReadFixture("Domain", "empty-label");

            CollectionAssert.AreEqual(content, domain.ToArray());
        }

        [Test]
        public void DomainWithSingleLabel() {
            Domain domain = new Domain(Helper.GetArray("www"));
            byte[] content = Helper.ReadFixture("Domain", "www-label");

            CollectionAssert.AreEqual(content, domain.ToArray());
        }

        [Test]
        public void DomainWithMultipleLabels() {
            Domain domain = new Domain(Helper.GetArray("www", "google", "com"));
            byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");

            CollectionAssert.AreEqual(content, domain.ToArray());
        }
    }
}
