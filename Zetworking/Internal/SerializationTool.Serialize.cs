using System;
using System.IO;
using System.Reflection;

namespace Zetworking.Internal;

internal static partial class SerializationTool
{
    internal static void Serialize(BinaryWriter writer, IZetPacket packet)
    {
        ArgumentNullException.ThrowIfNull(packet, nameof(packet));

        PropertyInfo[] properties = packet.GetType().GetProperties();
        foreach (var property in properties)
            if (property is { CanRead: true, CanWrite: true })
                WriteValue(writer, property.PropertyType, property.GetValue(packet));
    }

    private static void WriteValue(BinaryWriter writer, Type propertyType, object? propertyValue)
    {
        if (propertyType == typeof(bool))
        {
            var value = (bool)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(sbyte))
        {
            var value = (sbyte)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(byte))
        {
            var value = (byte)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(short))
        {
            var value = (short)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(ushort))
        {
            var value = (ushort)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(int))
        {
            var value = (int)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(uint))
        {
            var value = (uint)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(long))
        {
            var value = (long)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(ulong))
        {
            var value = (ulong)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(float))
        {
            var value = (float)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(double))
        {
            var value = (double)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(decimal))
        {
            var value = (decimal)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(char))
        {
            var value = (char)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(string))
        {
            var value = (string)propertyValue!;
            writer.Write(value);
        }
        else if (propertyType == typeof(DateTime))
        {
            var value = (DateTime)propertyValue!;
            writer.Write(value.ToBinary());
        }
        else
        {
            throw new NotSupportedException($"Type {propertyType} is not supported.");
        }
    }
}
