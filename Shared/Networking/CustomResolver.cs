using System.Runtime.InteropServices;
using Bergmann.Shared.Objects;
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using OpenTK.Mathematics;

namespace Bergmann.Shared;

/// <summary>
/// A custom resolver used for serializing and deserializing values in the message pack protocol.
/// For example of usages, see the building of the app in server. If a type is not part of this, you might as well add it.
/// It uses a <see cref="StandardResolver"/>, if it can't find the specified type.
/// </summary>
public class CustomResolver : IFormatterResolver {

    /// <summary>
    /// Returns an appropriate formatter from the type. Custom types with serialization just built for this project, it returns those,
    /// otherwise a bulit in resolver is used.
    /// </summary>
    /// <typeparam name="T">The type which shall be serialized.</typeparam>
    /// <returns>An object which supports serialization for type <typeparamref name="T"/> as specified by 
    /// <see cref="IMessagePackFormatter{T}"/>.</returns>
    public IMessagePackFormatter<T> GetFormatter<T>() {
        if (typeof(T) == typeof(Chunk))
            return (IMessagePackFormatter<T>)new ChunkFormatter();
        if (typeof(T) == typeof(Vector3))
            return (IMessagePackFormatter<T>)new Vector3Formatter();
        if (typeof(T) == typeof(Vector3i))
            return (IMessagePackFormatter<T>)new Vector3iFormatter();
        if (typeof(T) == typeof(Block))
            return (IMessagePackFormatter<T>)new BlockFormatter();

        return StandardResolver.Instance.GetFormatter<T>();
    }

    /// <summary>
    /// Serializs a <see cref="Vector3"/> by OpenTK. Since the serialization options are limited to writing ints, and not floats, 
    /// those are reinterpreted first. This shouldn't be a costly operation since it is not real conversion and purely
    /// happens on the stack.
    /// </summary>
    private class Vector3Formatter : IMessagePackFormatter<Vector3> {
        public Vector3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            Span<int> ints = stackalloc int[3];
            ints[0] = reader.ReadInt32();
            ints[1] = reader.ReadInt32();
            ints[2] = reader.ReadInt32();
            var floats = MemoryMarshal.Cast<int, float>(ints);
            return new() {
                X = floats[0],
                Y = floats[1],
                Z = floats[2],
            };
        }

        public void Serialize(ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options) {
            Span<float> floats = stackalloc float[3];
            floats[0] = value.X;
            floats[1] = value.Y;
            floats[2] = value.Z;
            var ints = MemoryMarshal.Cast<float, int>(floats);
            writer.WriteInt32(ints[0]);
            writer.WriteInt32(ints[1]);
            writer.WriteInt32(ints[2]);
        }
    }


    private class Vector3iFormatter : IMessagePackFormatter<Vector3i> {
        public Vector3i Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return new(
                reader.ReadInt32(),
                reader.ReadInt32(),
                reader.ReadInt32()
            );
        }

        public void Serialize(ref MessagePackWriter writer, Vector3i value, MessagePackSerializerOptions options) {
            writer.WriteInt32(value.X);
            writer.WriteInt32(value.Y);
            writer.WriteInt32(value.Z);
        }
    }

    
    private class BlockFormatter : IMessagePackFormatter<Block> {
        public Block Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            return reader.ReadInt32();
        }

        public void Serialize(ref MessagePackWriter writer, Block value, MessagePackSerializerOptions options) {
            writer.WriteInt32(value);
        }
    }


    /// <summary>
    /// Serializes a <see cref="Chunk"/>. This is subject to change if the chunk definition changes. Currently, all blocks a are written in a specific manner,
    /// then the key, which is the offset, is written too. There might be some room for improvements using streams, although only a little?
    /// </summary>
    private class ChunkFormatter : IMessagePackFormatter<Chunk> {
        public Chunk Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            Chunk result = new();
            result.Blocks = new int[16, 16, 16];

            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                        result.Blocks[x, y, z] = reader.ReadInt32();

            result.Key = reader.ReadInt64();
            return result;
        }

        public void Serialize(ref MessagePackWriter writer, Chunk value, MessagePackSerializerOptions options) {
            for (int x = 0; x < 16; x++)
                for (int y = 0; y < 16; y++)
                    for (int z = 0; z < 16; z++)
                        writer.WriteInt32(value.Blocks[x, y, z]);

            writer.WriteInt64(value.Key);
        }
    }
}