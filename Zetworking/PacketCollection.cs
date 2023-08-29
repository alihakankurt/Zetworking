using System.Buffers;
using System.Runtime.InteropServices;
using Zetworking.Internal;

namespace Zetworking;

public static class PacketCollection
{
    private static ushort _packetId;

    private static readonly IDictionary<ushort, Type> _packetTypes;

    static PacketCollection()
    {
        _packetId = 1;
        _packetTypes = new Dictionary<ushort, Type>();
    }

    public static void Register<T>()
    {
        _packetTypes.Add(_packetId++, typeof(T));
    }

    internal static byte[] Prepare(object obj)
    {
        ushort packetId = _packetTypes.FirstOrDefault(x => x.Value == obj.GetType()).Key;
        if (packetId == 0)
            throw new InvalidOperationException("Packet not registered");

        ushort size = SerializationTool.CalculateSize(obj);
        size += 2;
        if (size > ushort.MaxValue)
            throw new InvalidOperationException("Packet size is too big");

        var buffer = new byte[size + 2];
        MemoryMarshal.Write(buffer.AsSpan(0, 2), ref size);
        MemoryMarshal.Write(buffer.AsSpan(2, 4), ref packetId);
        SerializationTool.Serialize(obj, buffer.AsSpan(4));

        return buffer;
    }

    internal static object Resolve(ReadOnlySpan<byte> data, out Type packetType)
    {
        var packetId = MemoryMarshal.Read<ushort>(data);

        if (!_packetTypes.TryGetValue(packetId, out packetType!))
            throw new InvalidOperationException("Packet not registered");

        object result = SerializationTool.Deserialize(packetType, data[2..]);
        return result;
    }
}

