using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Zetworking.Internal;

namespace Zetworking;

public static class ZetPacketCollection
{
    private static ushort _nextPacketId;
    private static readonly Dictionary<ushort, Type> _registeredPackets;

    static ZetPacketCollection()
    {
        _nextPacketId = 1;
        _registeredPackets = [];
    }

    public static void Register(Type packetType)
    {
        if (!typeof(IZetPacket).IsAssignableFrom(packetType))
            throw new InvalidOperationException($"Packet must implement {nameof(IZetPacket)}.");

        _registeredPackets.Add(_nextPacketId, packetType);
        _nextPacketId += 1;
    }

    internal static byte[] Prepare(IZetPacket packet)
    {
        Type packetType = packet.GetType();
        ushort packetId = _registeredPackets.FirstOrDefault((pair) => pair.Value == packetType).Key;
        if (packetId == 0)
            throw new InvalidOperationException("Packets must be registered before sending.");

        using var stream = new MemoryStream(4);
        using var writer = new BinaryWriter(stream);
        stream.Position = 2;

        writer.Write(packetId);
        SerializationTool.Serialize(writer, packet);

        stream.Position = 0;
        writer.Write((ushort)stream.Length);

        return stream.ToArray();
    }

    internal static IZetPacket Resolve(byte[] data, out Type packetType)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        ushort packetSize = reader.ReadUInt16();
        ushort packetId = reader.ReadUInt16();

        if (!_registeredPackets.TryGetValue(packetId, out packetType!))
            throw new InvalidOperationException("Packet type could not be resolved.");

        SerializationTool.Deserialize(reader, packetType, out IZetPacket packet);
        return packet;
    }
}
