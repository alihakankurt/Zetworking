using System.Reflection;
using System.Text;

namespace Zetworking.Internal;

internal static partial class SerializationTool
{
    internal static ushort CalculateSize(object obj)
    {
        ushort size = 0;

        var properties = obj.GetType().GetProperties().Where(x => x.CanRead);
        foreach (var property in properties)
        {
            size += GetSize(obj, property);
        }

        return size;
    }

    private static ushort GetSize(object obj, PropertyInfo property)
    {
        Type type = property.PropertyType;
        if (type == typeof(string))
        {
            var @string = (string)property.GetValue(obj)!;
            int encoded = Encoding.UTF8.GetByteCount(@string);
            return (ushort)(encoded + 4);
        }
        else if (type == typeof(bool))
        {
            return 1;
        }
        else if (type == typeof(byte))
        {
            return 1;
        }
        else if (type == typeof(sbyte))
        {
            return 1;
        }
        else if (type == typeof(char))
        {
            return 2;
        }
        else if (type == typeof(short))
        {
            return 2;
        }
        else if (type == typeof(ushort))
        {
            return 2;
        }
        else if (type == typeof(int))
        {
            return 4;
        }
        else if (type == typeof(uint))
        {
            return 4;
        }
        else if (type == typeof(long))
        {
            return 8;
        }
        else if (type == typeof(ulong))
        {
            return 8;
        }
        else if (type == typeof(DateTime))
        {
            return 8;
        }
        else
        {
            return 0;
        }
    }
}
