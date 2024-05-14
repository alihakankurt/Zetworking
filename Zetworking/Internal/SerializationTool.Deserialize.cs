using System;
using System.IO;
using System.Reflection;

namespace Zetworking.Internal;

internal static partial class SerializationTool
{
    internal static void Deserialize(BinaryReader reader, Type packetType, out IZetPacket packet)
    {
        packet = Activator.CreateInstance(packetType) as IZetPacket
            ?? throw new InvalidOperationException($"Packet must implement {nameof(IZetPacket)}.");

        PropertyInfo[] properties = packetType.GetProperties();
        foreach (var property in properties)
            if (property is { CanRead: true, CanWrite: true })
                property.SetValue(packet, ReadValue(reader, property.PropertyType));
    }

    private static object ReadValue(BinaryReader reader, Type propertyType)
    {
        if (propertyType == typeof(bool))
        {
            return reader.ReadBoolean();
        }
        else if (propertyType == typeof(sbyte))
        {
            return reader.ReadSByte();
        }
        else if (propertyType == typeof(byte))
        {
            return reader.ReadByte();
        }
        else if (propertyType == typeof(short))
        {
            return reader.ReadInt16();
        }
        else if (propertyType == typeof(ushort))
        {
            return reader.ReadUInt16();
        }
        else if (propertyType == typeof(int))
        {
            return reader.ReadInt32();
        }
        else if (propertyType == typeof(uint))
        {
            return reader.ReadUInt32();
        }
        else if (propertyType == typeof(long))
        {
            return reader.ReadInt64();
        }
        else if (propertyType == typeof(ulong))
        {
            return reader.ReadUInt64();
        }
        else if (propertyType == typeof(float))
        {
            return reader.ReadSingle();
        }
        else if (propertyType == typeof(double))
        {
            return reader.ReadDouble();
        }
        else if (propertyType == typeof(decimal))
        {
            return reader.ReadDecimal();
        }
        else if (propertyType == typeof(char))
        {
            return reader.ReadChar();
        }
        else if (propertyType == typeof(string))
        {
            return reader.ReadString();
        }
        else if (propertyType == typeof(DateTime))
        {
            return DateTime.FromBinary(reader.ReadInt64());
        }
        else
        {
            throw new NotSupportedException($"Type {propertyType} is not supported.");
        }
    }
}
