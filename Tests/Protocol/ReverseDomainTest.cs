using System;
using System.Net;
using NUnit.Framework;
using DNS.Protocol;

namespace DNS.Tests.Protocol {
    [TestFixture]
    public class ReverseDomainTest {
        [Test]
        public void AllZeroIPv4() {
            IPAddress ip = IPAddress.Parse("0.0.0.0");
            Domain domain = Domain.PointerName(ip);

            Assert.AreEqual("0.0.0.0.in-addr.arpa", domain.ToString());
        }

        [Test]
        public void IPv4() {
            IPAddress ip = IPAddress.Parse("173.194.69.100");
            Domain domain = Domain.PointerName(ip);

            Assert.AreEqual("100.69.194.173.in-addr.arpa", domain.ToString());
        }

        [Test]
        public void AllZeroIPv6() {
            IPAddress ip = IPAddress.Parse("0000:0000:0000:0000:0000:0000:0000:0000");
            Domain domain = Domain.PointerName(ip);

            Assert.AreEqual("0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.ip6.arpa", domain.ToString());
        }

        [Test]
        public void IPv6() {
            IPAddress ip = IPAddress.Parse("2001:0db8:0000:0000:0000:0000:0567:89ab");
            Domain domain = Domain.PointerName(ip);

            Assert.AreEqual("b.a.9.8.7.6.5.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.0.8.b.d.0.1.0.0.2.ip6.arpa", domain.ToString());
        }
    }
}
