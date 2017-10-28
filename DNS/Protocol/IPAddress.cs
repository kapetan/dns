using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNS.Protocol
{
    public enum AddressFamily
    {
        AppleTalk = 0x10,
        Atm = 0x16,
        Banyan = 0x15,
        Ccitt = 10,
        Chaos = 5,
        Cluster = 0x18,
        DataKit = 9,
        DataLink = 13,
        DecNet = 12,
        Ecma = 8,
        FireFox = 0x13,
        HyperChannel = 15,
        Ieee12844 = 0x19,
        ImpLink = 3,
        InterNetwork = 2,
        InterNetworkV6 = 0x17,
        Ipx = 6,
        Irda = 0x1a,
        Iso = 7,
        Lat = 14,
        Max = 0x1d,
        NetBios = 0x11,
        NetworkDesigners = 0x1c,
        NS = 6,
        Osi = 7,
        Pup = 4,
        Sna = 11,
        Unix = 1,
        Unknown = -1,
        Unspecified = 0,
        VoiceView = 0x12
    }

    public class IPAddress
    {
        // Fields
        public static readonly IPAddress Any = new IPAddress(0);
        public static readonly IPAddress Broadcast = new IPAddress(0xffffffffL);
        internal const int IPv4AddressBytes = 4;
        internal const int IPv6AddressBytes = 0x10;
        public static readonly IPAddress IPv6Any = new IPAddress(new byte[0x10], 0L);
        public static readonly IPAddress IPv6Loopback;
        public static readonly IPAddress IPv6None;
        public static readonly IPAddress Loopback = new IPAddress(0x100007f);
        internal const long LoopbackMask = 0xffL;
        internal long m_Address;
        private AddressFamily m_Family;
        private int m_HashCode;
        private ushort[] m_Numbers;
        private long m_ScopeId;
        internal string m_ToString;
        public static readonly IPAddress None = Broadcast;
        internal const int NumberOfLabels = 8;

        // Methods
        static IPAddress()
        {
            byte[] address = new byte[0x10];
            address[15] = 1;
            IPv6Loopback = new IPAddress(address, 0L);
            IPv6None = new IPAddress(new byte[0x10], 0L);
        }

        internal IPAddress(int newAddress)
        {
            this.m_Family = AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            this.m_Address = newAddress & ((long)0xffffffffL);
        }

        public IPAddress(long newAddress)
        {
            this.m_Family = AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            if ((newAddress < 0L) || (newAddress > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("newAddress");
            }
            this.m_Address = newAddress;
        }

        public IPAddress(byte[] address)
        {
            this.m_Family = AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if ((address.Length != 4) && (address.Length != 0x10))
            {
                throw new ArgumentException("dns_bad_ip_address", "address");
            }
            if (address.Length == 4)
            {
                this.m_Family = AddressFamily.InterNetwork;
                this.m_Address = ((((address[3] << 0x18) | (address[2] << 0x10)) | (address[1] << 8)) | address[0]) & ((long)0xffffffffL);
            }
            else
            {
                this.m_Family = AddressFamily.InterNetworkV6;
                for (int i = 0; i < 8; i++)
                {
                    this.m_Numbers[i] = (ushort)((address[i * 2] * 0x100) + address[(i * 2) + 1]);
                }
            }
        }

        public IPAddress(byte[] address, long scopeid)
        {
            this.m_Family = AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.Length != 0x10)
            {
                throw new ArgumentException("dns_bad_ip_address", "address");
            }
            this.m_Family = AddressFamily.InterNetworkV6;
            for (int i = 0; i < 8; i++)
            {
                this.m_Numbers[i] = (ushort)((address[i * 2] * 0x100) + address[(i * 2) + 1]);
            }
            if ((scopeid < 0L) || (scopeid > 0xffffffffL))
            {
                throw new ArgumentOutOfRangeException("scopeid");
            }
            this.m_ScopeId = scopeid;
        }

        private IPAddress(ushort[] address, uint scopeid)
        {
            this.m_Family = AddressFamily.InterNetwork;
            this.m_Numbers = new ushort[8];
            this.m_Family = AddressFamily.InterNetworkV6;
            this.m_Numbers = address;
            this.m_ScopeId = scopeid;
        }

        public override bool Equals(object comparand) =>
            this.Equals(comparand, true);

        internal bool Equals(object comparandObj, bool compareScopeId)
        {
            IPAddress address = comparandObj as IPAddress;
            if (address == null)
            {
                return false;
            }
            if (this.m_Family != address.m_Family)
            {
                return false;
            }
            if (this.m_Family != AddressFamily.InterNetworkV6)
            {
                return (address.m_Address == this.m_Address);
            }
            for (int i = 0; i < 8; i++)
            {
                if (address.m_Numbers[i] != this.m_Numbers[i])
                {
                    return false;
                }
            }
            return ((address.m_ScopeId == this.m_ScopeId) || !compareScopeId);
        }

        public byte[] GetAddressBytes()
        {
            if (this.m_Family == AddressFamily.InterNetworkV6)
            {
                byte[] buffer = new byte[0x10];
                int num = 0;
                for (int i = 0; i < 8; i++)
                {
                    buffer[num++] = (byte)((this.m_Numbers[i] >> 8) & 0xff);
                    buffer[num++] = (byte)(this.m_Numbers[i] & 0xff);
                }
                return buffer;
            }
            return new byte[] { ((byte)this.m_Address), ((byte)(this.m_Address >> 8)), ((byte)(this.m_Address >> 0x10)), ((byte)(this.m_Address >> 0x18)) };
        }

        //public override int GetHashCode()
        //{
        //    if (this.m_Family != AddressFamily.InterNetworkV6)
        //    {
        //        return (int)this.m_Address;
        //    }
        //    if (this.m_HashCode == 0)
        //    {
        //        this.m_HashCode = StringComparer.CurrentCulture.GetHashCode(this.ToString());
        //    }
        //    return this.m_HashCode;
        //}

        public static short HostToNetworkOrder(short host) =>
            ((short)(((host & 0xff) << 8) | ((host >> 8) & 0xff)));

        public static int HostToNetworkOrder(int host) =>
            (((HostToNetworkOrder((short)host) & 0xffff) << 0x10) | (HostToNetworkOrder((short)(host >> 0x10)) & 0xffff));

        public static long HostToNetworkOrder(long host) =>
            ((long)(((HostToNetworkOrder((int)host) & 0xffffffffL) << 0x20) | (HostToNetworkOrder((int)(host >> 0x20)) & 0xffffffffL)));

        private static IPAddress InternalParse(string ipString, bool tryParse)
        {
            if (ipString.Contains("."))
               return new IPAddress(ipString.Split('.').Select(x => byte.Parse(x)).ToArray());
            else if (ipString.Contains(":"))
                return new IPAddress(ipString.Split(':').Select(x => byte.Parse(x)).ToArray());
            else if (ipString.Contains(","))
                return new IPAddress(ipString.Split(',').Select(x => byte.Parse(x)).ToArray());
            throw new NotSupportedException();
            //long num7;
            //if (ipString == null)
            //{
            //    if (!tryParse)
            //    {
            //        throw new ArgumentNullException("ipString");
            //    }
            //    return null;
            //}
            //if (ipString.IndexOf(':') != -1)
            //{
            //    Exception innerException = null;
            //    //if (Socket.OSSupportsIPv6)
            //    //{
            //    //    byte[] buffer = new byte[0x10];
            //    //    SocketAddress address = new SocketAddress(AddressFamily.InterNetworkV6, 0x1c);
            //    //    if (UnsafeNclNativeMethods.OSSOCK.WSAStringToAddress(ipString, AddressFamily.InterNetworkV6, IntPtr.Zero, address.m_Buffer, ref address.m_Size) == SocketError.Success)
            //    //    {
            //    //        for (int i = 0; i < 0x10; i++)
            //    //        {
            //    //            buffer[i] = address[i + 8];
            //    //        }
            //    //        return new IPAddress(buffer, (((address[0x1b] << 0x18) + (address[0x1a] << 0x10)) + (address[0x19] << 8)) + address[0x18]);
            //    //    }
            //    //    if (tryParse)
            //    //    {
            //    //        return null;
            //    //    }
            //    //    innerException = new SocketException();
            //    //}
            //    //else
            //    //{
            //        int start = 0;
            //        if (ipString[0] != '[')
            //        {
            //            ipString = ipString + "]";
            //        }
            //        else
            //        {
            //            start = 1;
            //        }
            //        int end = ipString.Length;
            //        fixed (char* str = ((char*)ipString))
            //        {
            //            char* name = str;
            //            if (name != null)
            //            {
            //                name += RuntimeHelpers.OffsetToStringData;
            //            }
            //            if (IPv6AddressHelper.IsValidStrict(name, start, ref end) || (end != ipString.Length))
            //            {
            //                uint num5;
            //                ushort[] numArray = new ushort[8];
            //                string scopeId = null;
            //                fixed (ushort* numRef = numArray)
            //                {
            //                    IPv6AddressHelper.Parse(ipString, numRef, 0, ref scopeId);
            //                }
            //                if ((scopeId == null) || (scopeId.Length == 0))
            //                {
            //                    return new IPAddress(numArray, 0);
            //                }
            //                if (uint.TryParse(scopeId.Substring(1), NumberStyles.None, null, out num5))
            //                {
            //                    return new IPAddress(numArray, num5);
            //                }
            //            }
            //        }
            //        if (tryParse)
            //        {
            //            return null;
            //        }
            //        innerException = new Exception("SocketError.InvalidArgument");
            //    //}
            //    throw new FormatException("dns_bad_ip_address", innerException);
            //}
            //Socket.InitializeSockets();
            //int length = ipString.Length;
            //fixed (char* str3 = ((char*)ipString))
            //{
            //    char* chPtr2 = str3;
            //    if (chPtr2 != null)
            //    {
            //        chPtr2 += RuntimeHelpers.OffsetToStringData;
            //    }
            //    num7 = IPv4AddressHelper.ParseNonCanonical(chPtr2, 0, ref length, true);
            //}
            //if ((num7 == -1L) || (length != ipString.Length))
            //{
            //    if (!tryParse)
            //    {
            //        throw new FormatException(SR.GetString("dns_bad_ip_address"));
            //    }
            //    return null;
            //}
            //return new IPAddress(((num7 & 0xffL) << 0x18) | (((num7 & 0xff00L) << 8) | (((num7 & 0xff0000L) >> 8) | ((long)((num7 & 0xff000000L) >> 0x18)))));
        }

        public static bool IsLoopback(IPAddress address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }
            if (address.m_Family == AddressFamily.InterNetworkV6)
            {
                return address.Equals(IPv6Loopback);
            }
            return ((address.m_Address & 0xffL) == (Loopback.m_Address & 0xffL));
        }

        public IPAddress MapToIPv4()
        {
            if (this.AddressFamily == AddressFamily.InterNetwork)
            {
                return this;
            }
            return new IPAddress((long)((ulong)((((this.m_Numbers[6] & 0xff00) >> 8) | ((this.m_Numbers[6] & 0xff) << 8)) | ((((this.m_Numbers[7] & 0xff00) >> 8) | ((this.m_Numbers[7] & 0xff) << 8)) << 0x10))));
        }

        public IPAddress MapToIPv6()
        {
            if (this.AddressFamily == AddressFamily.InterNetworkV6)
            {
                return this;
            }
            ushort[] address = new ushort[8];
            address[5] = 0xffff;
            address[6] = (ushort)(((this.m_Address & 0xff00L) >> 8) | ((this.m_Address & 0xffL) << 8));
            address[7] = (ushort)(((ulong)(this.m_Address & 0xff000000L) >> 0x18) | ((ulong)((this.m_Address & 0xff0000L) >> 8)));
            return new IPAddress(address, 0);
        }

        public static short NetworkToHostOrder(short network) =>
            HostToNetworkOrder(network);

        public static int NetworkToHostOrder(int network) =>
            HostToNetworkOrder(network);

        public static long NetworkToHostOrder(long network) =>
            HostToNetworkOrder(network);

        public static IPAddress Parse(string ipString) =>
            InternalParse(ipString, false);

        internal IPAddress Snapshot()
        {
            AddressFamily family = this.m_Family;
            if (family != AddressFamily.InterNetwork)
            {
                if (family != AddressFamily.InterNetworkV6)
                {
                    throw new Exception();
                }
                return new IPAddress(this.m_Numbers, (uint)this.m_ScopeId);
            }
            return new IPAddress(this.m_Address);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (var item in GetAddressBytes())
            {
                builder.Append(item);
                builder.Append(":");
            }
            return builder.ToString().Trim(':');
        }

        //public static bool TryParse(string ipString, out IPAddress address)
        //{
        //    address = InternalParse(ipString, true);
        //    return (address != null);
        //}

        // Properties
        public long Address
        {
            get
            {
                if (this.m_Family == AddressFamily.InterNetworkV6)
                {
                    throw new Exception("SocketError.OperationNotSupported");
                }
                return this.m_Address;
            }
            set
            {
                if (this.m_Family == AddressFamily.InterNetworkV6)
                {
                    throw new Exception("SocketError.OperationNotSupported");
                }
                if (this.m_Address != value)
                {
                    this.m_ToString = null;
                    this.m_Address = value;
                }
            }
        }

        public AddressFamily AddressFamily =>
            this.m_Family;

        internal bool IsBroadcast
        {
            get
            {
                if (this.m_Family == AddressFamily.InterNetworkV6)
                {
                    return false;
                }
                return (this.m_Address == Broadcast.m_Address);
            }
        }

        public bool IsIPv4MappedToIPv6
        {
            get
            {
                if (this.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    return false;
                }
                for (int i = 0; i < 5; i++)
                {
                    if (this.m_Numbers[i] != 0)
                    {
                        return false;
                    }
                }
                return (this.m_Numbers[5] == 0xffff);
            }
        }

        public bool IsIPv6LinkLocal =>
            ((this.m_Family == AddressFamily.InterNetworkV6) && ((this.m_Numbers[0] & 0xffc0) == 0xfe80));

        public bool IsIPv6Multicast =>
            ((this.m_Family == AddressFamily.InterNetworkV6) && ((this.m_Numbers[0] & 0xff00) == 0xff00));

        public bool IsIPv6SiteLocal =>
            ((this.m_Family == AddressFamily.InterNetworkV6) && ((this.m_Numbers[0] & 0xffc0) == 0xfec0));

        public bool IsIPv6Teredo =>
            (((this.m_Family == AddressFamily.InterNetworkV6) && (this.m_Numbers[0] == 0x2001)) && (this.m_Numbers[1] == 0));

        public long ScopeId
        {
            get
            {
                if (this.m_Family == AddressFamily.InterNetwork)
                {
                    throw new Exception("SocketError.OperationNotSupported");
                }
                return this.m_ScopeId;
            }
            set
            {
                if (this.m_Family == AddressFamily.InterNetwork)
                {
                    throw new Exception("SocketError.OperationNotSupported");
                }
                if ((value < 0L) || (value > 0xffffffffL))
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                if (this.m_ScopeId != value)
                {
                    this.m_Address = value;
                    this.m_ScopeId = value;
                }
            }
        }
    }

}
