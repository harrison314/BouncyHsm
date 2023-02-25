using BouncyHsm.Core.Services.Contracts.P11;
using MessagePack;
using System.Buffers;

namespace BouncyHsm.Core.Services.Contracts.Entities;


[MessagePackFormatter(typeof(StorageObjectMementoMessagePackFormatter))]
public class StorageObjectMemento : Entity
{
    public Dictionary<CKA, IAttributeValue> Values
    {
        get;
    }

    public static StorageObjectMemento FromInstance(byte[] content)
    {
        System.Diagnostics.Debug.Assert(content != null);

        return MessagePackSerializer.Deserialize<StorageObjectMemento>(content);
    }

    public StorageObjectMemento()
    {
        this.Values = new Dictionary<CKA, IAttributeValue>();
    }

    public StorageObjectMemento(Guid id, IReadOnlyDictionary<CKA, IAttributeValue> values)
    {
        System.Diagnostics.Debug.Assert(id != Guid.Empty);
        System.Diagnostics.Debug.Assert(values != null);

        this.Id = id;
        this.Values = new Dictionary<CKA, IAttributeValue>(values);
    }

    public byte[] ToByteArray()
    {
        return MessagePackSerializer.Serialize<StorageObjectMemento>(this);
    }

    public void WriteTo(IBufferWriter<byte> writer)
    {
        MessagePackSerializer.Serialize(writer, this);
    }
}
