using Xunit;
using DNS.Protocol;

namespace DNS.Tests.Protocol {

    public class CompareDomainTest {
        [Fact]
        public void SameDomainInstance() {
            Domain domain = new Domain(Helper.GetArray("www"));
            Assert.Equal(0, domain.CompareTo(domain));
        }

        [Fact]
        public void SameDomainsWithSingleLabelDifferentCasing() {
            Domain a = new Domain(Helper.GetArray("www"));
            Domain b = new Domain(Helper.GetArray("WWW"));

            Assert.Equal(0, a.CompareTo(b));
        }

        [Fact]
        public void SameDomainsWithSingleLabelNonAlphabeticCodes() {
            Domain a = new Domain(Helper.GetArray<byte[]>(
                Helper.GetArray<byte>(119, 0, 119)
            ));
            Domain b = new Domain(Helper.GetArray<byte[]>(
                Helper.GetArray<byte>(119, 0, 119)
            ));

            Assert.Equal(0, a.CompareTo(b));
        }

        [Fact]
        public void SameDomainsWithMultipleLabels() {
            Domain a = new Domain(Helper.GetArray<byte[]>(
                Helper.GetArray<byte>(119, 119, 119),
                Helper.GetArray<byte>(103, 111, 111, 103, 108, 101),
                Helper.GetArray<byte>(99, 0, 109)
            ));
            Domain b = new Domain(Helper.GetArray<byte[]>(
                Helper.GetArray<byte>(87, 87, 87),
                Helper.GetArray<byte>(103, 79, 79, 103, 108, 101),
                Helper.GetArray<byte>(99, 0, 77)
            ));

            Assert.Equal(0, a.CompareTo(b));
        }

        [Fact]
        public void DifferentDomainsWithSingleLabelSameLength() {
            Domain a = new Domain(Helper.GetArray("aww"));
            Domain b = new Domain(Helper.GetArray("www"));

            Assert.True(a.CompareTo(b) < 0);
            Assert.True(b.CompareTo(a) > 0);
        }

        [Fact]
        public void DifferentDomainsWithSingleLabelDifferentLength() {
            Domain a = new Domain(Helper.GetArray("ww"));
            Domain b = new Domain(Helper.GetArray("www"));

            Assert.True(a.CompareTo(b) < 0);
            Assert.True(b.CompareTo(a) > 0);
        }

        [Fact]
        public void DifferentDomainsWithSingleLabelNonAlphabeticCodes() {
            Domain a = new Domain(Helper.GetArray<byte[]>(
                Helper.GetArray<byte>(119, 0, 119)
            ));
            Domain b = new Domain(Helper.GetArray<byte[]>(
                Helper.GetArray<byte>(119, 119, 119)
            ));

            Assert.True(a.CompareTo(b) < 0);
            Assert.True(b.CompareTo(a) > 0);
        }

        [Fact]
        public void DifferentDomainsWithMultipleLabelsDifferentAmount() {
            Domain a = new Domain(Helper.GetArray("www"));
            Domain b = new Domain(Helper.GetArray("www", "google"));

            Assert.True(a.CompareTo(b) < 0);
            Assert.True(b.CompareTo(a) > 0);
        }
    }
}
