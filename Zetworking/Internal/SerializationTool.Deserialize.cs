using System.Text;
using System.Runtime.InteropServices;

namespace Zetworking.Internal;

internal static partial class SerializationTool
{
    internal static object Deserialize(Type type, ReadOnlySpan<byte> data)
    {
        var obj = Activator.CreateInstance(type);
        ushort read = 0;

        var properties = type.GetProperties().Where(x => x.CanWrite);
        foreach (var property in properties)
        {
            read += ReadValue(data[read..], property.PropertyType, out var value);
            property.SetValue(obj, value);
        }

        return obj!;
    }

    private static ushort ReadValue(ReadOnlySpan<byte> data, Type type, out object? value)
    {
        if (type == typeof(string))
        {
            int encoded = MemoryMarshal.Read<int>(data);
            value = Encoding.UTF8.GetString(data[4..(4 + encoded)]);
            return (ushort)(encoded + 4);
        }
        else if (type == typeof(bool))
        {
            value = MemoryMarshal.Read<bool>(data);
            return 1;
        }
        else if (type == typeof(byte))
        {
            value = MemoryMarshal.Read<byte>(data);
            return 1;
        }
        else if (type == typeof(sbyte))
        {
            value = MemoryMarshal.Read<sbyte>(data);
            return 1;
        }
        else if (type == typeof(char))
        {
            value = MemoryMarshal.Read<char>(data);
            return 2;
        }
        else if (type == typeof(short))
        {
            value = MemoryMarshal.Read<short>(data);
            return 2;
        }
        else if (type == typeof(ushort))
        {
            value = MemoryMarshal.Read<ushort>(data);
            return 2;
        }
        else if (type == typeof(int))
        {
            value = MemoryMarshal.Read<int>(data);
            return 4;
        }
        else if (type == typeof(uint))
        {
            value = MemoryMarshal.Read<uint>(data);
            return 4;
        }
        else if (type == typeof(long))
        {
            value = MemoryMarshal.Read<long>(data);
            return 8;
        }
        else if (type == typeof(ulong))
        {
            value = MemoryMarshal.Read<ulong>(data);
            return 8;
        }
        else if (type == typeof(DateTime))
        {
            value = MemoryMarshal.Read<DateTime>(data);
            return 8;
        }
        else
        {
            value = default;
            return 0;
        }
    }
}
