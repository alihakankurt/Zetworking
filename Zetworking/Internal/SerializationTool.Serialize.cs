using System.Text;
using System.Runtime.InteropServices;

namespace Zetworking.Internal;

internal static partial class SerializationTool
{
    internal static void Serialize(object obj, Span<byte> buffer)
    {
        ushort written = 0;

        var properties = obj.GetType().GetProperties().Where(x => x.CanRead);
        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            written += WriteValue(buffer[written..], property.PropertyType, value);
        }
    }

    private static ushort WriteValue(Span<byte> buffer, Type type, object? value)
    {
        if (type == typeof(string))
        {
            var @string = (string)value!;
            int encoded = Encoding.UTF8.GetBytes(@string, buffer[4..]);
            MemoryMarshal.Write(buffer, ref encoded);
            return (ushort)(encoded + 4);
        }
        else if (type == typeof(bool))
        {
            var @bool = (bool)value!;
            MemoryMarshal.Write(buffer, ref @bool);
            return 1;
        }
        else if (type == typeof(byte))
        {
            var @byte = (byte)value!;
            MemoryMarshal.Write(buffer, ref @byte);
            return 1;
        }
        else if (type == typeof(sbyte))
        {
            var @sbyte = (sbyte)value!;
            MemoryMarshal.Write(buffer, ref @sbyte);
            return 1;
        }
        else if (type == typeof(char))
        {
            var @char = (char)value!;
            MemoryMarshal.Write(buffer, ref @char);
            return 2;
        }
        else if (type == typeof(short))
        {
            var @short = (short)value!;
            MemoryMarshal.Write(buffer, ref @short);
            return 2;
        }
        else if (type == typeof(ushort))
        {
            var @ushort = (ushort)value!;
            MemoryMarshal.Write(buffer, ref @ushort);
            return 2;
        }
        else if (type == typeof(int))
        {
            var @int = (int)value!;
            MemoryMarshal.Write(buffer, ref @int);
            return 4;
        }
        else if (type == typeof(uint))
        {
            var @uint = (uint)value!;
            MemoryMarshal.Write(buffer, ref @uint);
            return 4;
        }
        else if (type == typeof(long))
        {
            var @long = (long)value!;
            MemoryMarshal.Write(buffer, ref @long);
            return 8;
        }
        else if (type == typeof(ulong))
        {
            var @ulong = (ulong)value!;
            MemoryMarshal.Write(buffer, ref @ulong);
            return 8;
        }
        else if (type == typeof(DateTime))
        {
            var dateTime = (DateTime)value!;
            MemoryMarshal.Write(buffer, ref dateTime);
            return 8;
        }
        else
        {
            return 0;
        }
    }
}
