using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Kapetan.Dns.Marshalling
{
    public static class Struct
    {
        private static byte[] ConvertEndian(Type type, byte[] data)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            EndianAttribute endian = null;

            if (type.IsDefined(typeof(EndianAttribute), false))
            {
                endian = (EndianAttribute)type.GetCustomAttributes(typeof(EndianAttribute), false)[0];
            }

            foreach (var field in fields)
            {
                if (endian == null && !field.IsDefined(typeof(EndianAttribute), false))
                {
                    continue;
                }

                var offset = Marshal.OffsetOf(type, field.Name).ToInt32();
                var length = Marshal.SizeOf(field.FieldType);
                endian = endian ?? (EndianAttribute)field.GetCustomAttributes(typeof(EndianAttribute), false)[0];

                if (endian.Endianness == Endianness.Big && BitConverter.IsLittleEndian || endian.Endianness == Endianness.Little && !BitConverter.IsLittleEndian)
                {
                    Array.Reverse(data, offset, length);
                }
            }

            return data;
        }

        public static T GetStruct<T>(byte[] data) where T : struct
        {
            return GetStruct<T>(data, 0, data.Length);
        }

        public static T GetStruct<T>(byte[] data, int offset, int length) where T : struct
        {
            var buffer = new byte[length];
            Array.Copy(data, offset, buffer, 0, buffer.Length);

            var handle = GCHandle.Alloc(ConvertEndian(typeof(T), buffer), GCHandleType.Pinned);

            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public static byte[] GetBytes<T>(T obj) where T : struct
        {
            var data = new byte[Marshal.SizeOf(obj)];
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);

            try
            {
                Marshal.StructureToPtr(obj, handle.AddrOfPinnedObject(), false);
                return ConvertEndian(typeof(T), data);
            }
            finally
            {
                handle.Free();
            }
        }
    }
}
