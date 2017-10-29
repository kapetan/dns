using System;
using System.Collections.Generic;
using System.Text;

namespace DNS.Protocol
{
    public class SocketAddress
    {
        // Fields
        internal const int IPv4AddressSize = 0x10;
        internal const int IPv6AddressSize = 0x1c;
        internal byte[] m_Buffer;
        private bool m_changed;
        private int m_hash;
        internal int m_Size;
        private const int MaxSize = 0x20;
        private const int WriteableOffset = 2;

        // Methods
        internal SocketAddress(IPAddress ipAddress) : this(ipAddress.AddressFamily, (ipAddress.AddressFamily == AddressFamily.InterNetwork) ? 0x10 : 0x1c)
        {
            this.m_Buffer[2] = 0;
            this.m_Buffer[3] = 0;
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                this.m_Buffer[4] = 0;
                this.m_Buffer[5] = 0;
                this.m_Buffer[6] = 0;
                this.m_Buffer[7] = 0;
                long scopeId = ipAddress.ScopeId;
                this.m_Buffer[0x18] = (byte)scopeId;
                this.m_Buffer[0x19] = (byte)(scopeId >> 8);
                this.m_Buffer[0x1a] = (byte)(scopeId >> 0x10);
                this.m_Buffer[0x1b] = (byte)(scopeId >> 0x18);
                byte[] addressBytes = ipAddress.GetAddressBytes();
                for (int i = 0; i < addressBytes.Length; i++)
                {
                    this.m_Buffer[8 + i] = addressBytes[i];
                }
            }
            else
            {
                this.m_Buffer[4] = (byte)ipAddress.m_Address;
                this.m_Buffer[5] = (byte)(ipAddress.m_Address >> 8);
                this.m_Buffer[6] = (byte)(ipAddress.m_Address >> 0x10);
                this.m_Buffer[7] = (byte)(ipAddress.m_Address >> 0x18);
            }
        }

        public SocketAddress(AddressFamily family) : this(family, 0x20)
        {
        }

        internal SocketAddress(IPAddress ipaddress, int port) : this(ipaddress)
        {
            this.m_Buffer[2] = (byte)(port >> 8);
            this.m_Buffer[3] = (byte)port;
        }

        public SocketAddress(AddressFamily family, int size)
        {
            this.m_changed = true;
            if (size < 2)
            {
                throw new ArgumentOutOfRangeException("size");
            }
            this.m_Size = size;
            this.m_Buffer = new byte[((size / IntPtr.Size) + 2) * IntPtr.Size];
            this.m_Buffer[0] = (byte)family;
            this.m_Buffer[1] = (byte)(((int)family) >> 8);
        }

        internal void CopyAddressSizeIntoBuffer()
        {
            this.m_Buffer[this.m_Buffer.Length - IntPtr.Size] = (byte)this.m_Size;
            this.m_Buffer[(this.m_Buffer.Length - IntPtr.Size) + 1] = (byte)(this.m_Size >> 8);
            this.m_Buffer[(this.m_Buffer.Length - IntPtr.Size) + 2] = (byte)(this.m_Size >> 0x10);
            this.m_Buffer[(this.m_Buffer.Length - IntPtr.Size) + 3] = (byte)(this.m_Size >> 0x18);
        }

        public override bool Equals(object comparand)
        {
            SocketAddress address = comparand as SocketAddress;
            if ((address == null) || (this.Size != address.Size))
            {
                return false;
            }
            for (int i = 0; i < this.Size; i++)
            {
                if (this[i] != address[i])
                {
                    return false;
                }
            }
            return true;
        }

        internal int GetAddressSizeOffset() =>
            (this.m_Buffer.Length - IntPtr.Size);

        public override int GetHashCode()
        {
            if (this.m_changed)
            {
                this.m_changed = false;
                this.m_hash = 0;
                int num2 = this.Size & -4;
                int index = 0;
                while (index < num2)
                {
                    this.m_hash ^= ((this.m_Buffer[index] | (this.m_Buffer[index + 1] << 8)) | (this.m_Buffer[index + 2] << 0x10)) | (this.m_Buffer[index + 3] << 0x18);
                    index += 4;
                }
                if ((this.Size & 3) != 0)
                {
                    int num3 = 0;
                    int num4 = 0;
                    while (index < this.Size)
                    {
                        num3 |= this.m_Buffer[index] << num4;
                        num4 += 8;
                        index++;
                    }
                    this.m_hash ^= num3;
                }
            }
            return this.m_hash;
        }

        internal IPAddress GetIPAddress()
        {
            if (this.Family == AddressFamily.InterNetworkV6)
            {
                byte[] address = new byte[0x10];
                for (int i = 0; i < address.Length; i++)
                {
                    address[i] = this.m_Buffer[i + 8];
                }
                return new IPAddress(address, (((this.m_Buffer[0x1b] << 0x18) + (this.m_Buffer[0x1a] << 0x10)) + (this.m_Buffer[0x19] << 8)) + this.m_Buffer[0x18]);
            }
            if (this.Family != AddressFamily.InterNetwork)
            {
                throw new Exception("SocketError.AddressFamilyNotSupported");
            }
            return new IPAddress(((((this.m_Buffer[4] & 0xff) | ((this.m_Buffer[5] << 8) & 0xff00)) | ((this.m_Buffer[6] << 0x10) & 0xff0000)) | (this.m_Buffer[7] << 0x18)) & ((long)0xffffffffL));
        }

        internal IPEndPoint GetIPEndPoint()
        {
            IPAddress iPAddress = this.GetIPAddress();
            return new IPEndPoint(iPAddress, ((this.m_Buffer[2] << 8) & 0xff00) | this.m_Buffer[3]);
        }

        internal void SetSize(IntPtr ptr)
        {
            this.m_Size = ptr.ToInt32();
        }

        //public override string ToString()
        //{
        //    StringBuilder builder = new StringBuilder();
        //    for (int i = 2; i < this.Size; i++)
        //    {
        //        if (i > 2)
        //        {
        //            builder.Append(",");
        //        }
        //        builder.Append(this[i].ToString(NumberFormatInfo.InvariantInfo));
        //    }
        //    string[] textArray1 = new string[] { this.Family.ToString(), ":", this.Size.ToString(NumberFormatInfo.InvariantInfo), ":{", builder.ToString(), "}" };
        //    return string.Concat(textArray1);
        //}

        // Properties
        public AddressFamily Family
        {
            get
            {
                int num = this.m_Buffer[0] | (this.m_Buffer[1] << 8);
                return (AddressFamily)num;
            }
        }

        public byte this[int offset]
        {
            get
            {
                if ((offset < 0) || (offset >= this.Size))
                {
                    throw new IndexOutOfRangeException();
                }
                return this.m_Buffer[offset];
            }
            set
            {
                if ((offset < 0) || (offset >= this.Size))
                {
                    throw new IndexOutOfRangeException();
                }
                if (this.m_Buffer[offset] != value)
                {
                    this.m_changed = true;
                }
                this.m_Buffer[offset] = value;
            }
        }

        public int Size =>
            this.m_Size;
    }
}
