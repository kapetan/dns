using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DNS.Protocol
{
    public abstract class EndPoint
    {
        // Methods
        public abstract EndPoint Create(SocketAddress socketAddress);
        public abstract SocketAddress Serialize();

        // Properties
        public virtual AddressFamily AddressFamily { get; }
    }

    public class IPEndPoint : EndPoint
    {
        // Fields
        internal static IPEndPoint Any = new IPEndPoint(IPAddress.Any, 0);
        internal const int AnyPort = 0;
        internal static IPEndPoint IPv6Any = new IPEndPoint(IPAddress.IPv6Any, 0);
        private IPAddress m_Address;
        private int m_Port;
        public const int MaxPort = 0xffff;
        public const int MinPort = 0;

        // Methods
        public IPEndPoint(long address, int port)
        {
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.m_Port = port;
            this.m_Address = new IPAddress(address);
        }

        public IPEndPoint(IPAddress address, int port)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (!ValidationHelper.ValidateTcpPort(port))
            {
                throw new ArgumentOutOfRangeException("port");
            }
            this.m_Port = port;
            this.m_Address = address;
        }

        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress.Family != this.AddressFamily)
            {
                object[] args = new object[] { socketAddress.Family.ToString(), base.GetType().FullName, this.AddressFamily.ToString() };
                throw new ArgumentException("net_InvalidAddressFamily", "socketAddress");
            }
            if (socketAddress.Size < 8)
            {
                object[] objArray2 = new object[] { socketAddress.GetType().FullName, base.GetType().FullName };
                throw new ArgumentException("net_InvalidSocketAddressSize", "socketAddress");
            }
            return socketAddress.GetIPEndPoint();
        }

        public override bool Equals(object comparand) =>
            ((comparand is IPEndPoint) && (((IPEndPoint)comparand).m_Address.Equals(this.m_Address) && (((IPEndPoint)comparand).m_Port == this.m_Port)));

        public override int GetHashCode() =>
            (this.m_Address.GetHashCode() ^ this.m_Port);

        public override SocketAddress Serialize() =>
            new SocketAddress(this.Address, this.Port);

        internal IPEndPoint Snapshot() =>
            new IPEndPoint(this.Address.Snapshot(), this.Port);

        public override string ToString()
        {
            string str;
            if (this.m_Address.AddressFamily == AddressFamily.InterNetworkV6)
            {
                str = "[{0}]:{1}";
            }
            else
            {
                str = "{0}:{1}";
            }
            return string.Format(str, this.m_Address.ToString(), this.Port.ToString(NumberFormatInfo.InvariantInfo));
        }

        // Properties
        public IPAddress Address
        {
            get =>
                this.m_Address;
            set
            {
                this.m_Address = value;
            }
        }

        public override AddressFamily AddressFamily =>
            this.m_Address.AddressFamily;

        public int Port
        {
            get =>
                this.m_Port;
            set
            {
                if (!ValidationHelper.ValidateTcpPort(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.m_Port = value;
            }
        }
#if (!PORTABLE)
        public static implicit operator System.Net.IPEndPoint(IPEndPoint ep)
        {
            return new System.Net.IPEndPoint(ep.Address.Address, ep.Port);
        }
#endif
    }


}
