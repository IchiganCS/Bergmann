using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking.Resolvers;

/// <summary>
/// A custom resolver used for serializing and deserializing values in the message pack protocol. 
/// The dynamic jit formatter from <see cref="DynamicObjectResolver"/> is not happy with the recursive properties
/// of different math types from OpenTK. 
/// </summary>
public class OpenTKResolver : IFormatterResolver {

    public static OpenTKResolver Instance { get; set; } = new();

    /// <summary>
    /// Returns an appropriate formatter from the type. 
    /// </summary>
    /// <typeparam name="T">The type which shall be serialized.</typeparam>
    /// <returns>An object which supports serialization for type <typeparamref name="T"/> as specified by 
    /// <see cref="IMessagePackFormatter{T}"/>.</returns>
    public IMessagePackFormatter<T> GetFormatter<T>() {
        if (typeof(T) == typeof(Vector3))
            return (IMessagePackFormatter<T>)new Vector3Formatter();
        if (typeof(T) == typeof(Vector3i))
            return (IMessagePackFormatter<T>)new Vector3iFormatter();

        return null!;
    }


    private class Vector3Formatter : IMessagePackFormatter<Vector3> {
        public unsafe Vector3 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) {
            int int1 = reader.ReadInt32();
            int int2 = reader.ReadInt32();
            int int3 = reader.ReadInt32();
            return new() {
                X = *(float*)&int1,
                Y = *(float*)&int2,
                Z = *(float*)&int3,
            };
        }

        public unsafe void Serialize(ref MessagePackWriter writer, Vector3 value, MessagePackSerializerOptions options) {
            writer.WriteInt32(*(int*)&value.X);
            writer.WriteInt32(*(int*)&value.Y);
            writer.WriteInt32(*(int*)&value.Z);
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
}