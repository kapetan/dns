using System;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class ParseDomainTest {
        [Test]
        public void EmptyDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "empty-label");
            Domain domain = Domain.FromArray(content, 0, out endOffset);

            Assert.AreEqual("", domain.ToString());
            Assert.AreEqual(1, domain.Size);
            Assert.AreEqual(1, endOffset);
        }

        [Test]
        public void DomainWithSingleLabel() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "www-label");
            Domain domain = Domain.FromArray(content, 0, out endOffset);

            Assert.AreEqual("www", domain.ToString());
            Assert.AreEqual(5, domain.Size);
            Assert.AreEqual(5, endOffset);
        }

        [Test]
        public void DomainWithMultipleLabels() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "www.google.com-label");
            Domain domain = Domain.FromArray(content, 0, out endOffset);

            Assert.AreEqual("www.google.com", domain.ToString());
            Assert.AreEqual(16, domain.Size);
            Assert.AreEqual(16, endOffset);
        }

        [Test]
        public void DomainWithMultipleLabelsPreceededByHeader() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "empty-header_www.google.com-label");
            Domain domain = Domain.FromArray(content, 12, out endOffset);

            Assert.AreEqual("www.google.com", domain.ToString());
            Assert.AreEqual(16, domain.Size);
            Assert.AreEqual(28, endOffset);
        }

        [Test]
        public void EmptyPointerDomain() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "empty-pointer");
            Domain domain = Domain.FromArray(content, 1, out endOffset);

            Assert.AreEqual("", domain.ToString());
            Assert.AreEqual(1, domain.Size);
            Assert.AreEqual(3, endOffset);
        }

        [Test]
        public void PointerDomainWithSingleLabel() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "www-pointer");
            Domain domain = Domain.FromArray(content, 5, out endOffset);

            Assert.AreEqual("www", domain.ToString());
            Assert.AreEqual(5, domain.Size);
            Assert.AreEqual(7, endOffset);
        }

        [Test]
        public void PointerDomainWithMultipleLabels() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "www.google.com-pointer");
            Domain domain = Domain.FromArray(content, 16, out endOffset);

            Assert.AreEqual("www.google.com", domain.ToString());
            Assert.AreEqual(16, domain.Size);
            Assert.AreEqual(18, endOffset);
        }

        [Test]
        public void PointerDomainWithMultipleLabelsPreceededByHeader() {
            int endOffset = 0;
            byte[] content = Helper.ReadFixture("Domain", "empty-header_www.google.com-pointer");
            Domain domain = Domain.FromArray(content, 28, out endOffset);

            Assert.AreEqual("www.google.com", domain.ToString());
            Assert.AreEqual(16, domain.Size);
            Assert.AreEqual(30, endOffset);
        }
    }
}
