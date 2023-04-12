using BouncyHsm.Core.Services.Contracts.P11;
using MessagePack;
using MessagePack.Formatters;
using System.Buffers;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal class StorageObjectMementoMessagePackFormatter : IMessagePackFormatter<StorageObjectMemento>
{
    public StorageObjectMementoMessagePackFormatter()
    {

    }

    public void Serialize(ref MessagePackWriter writer, StorageObjectMemento value, MessagePackSerializerOptions options)
    {
        writer.WriteArrayHeader(3);
        writer.Write(1); // Version
        writer.Write(value.Id.ToByteArray());
        writer.WriteMapHeader(value.Values.Count);

        foreach ((CKA attrType, IAttributeValue attrVal) in value.Values)
        {
            writer.WriteArrayHeader(3);
            writer.Write((uint)attrType);
            writer.Write((uint)attrVal.TypeTag);

            switch (attrVal.TypeTag)
            {
                case AttrTypeTag.ByteArray:
                    writer.Write(attrVal.AsByteArray());
                    break;

                case AttrTypeTag.CkAttributeArray:
                    throw new NotImplementedException();

                case AttrTypeTag.CkBool:
                    writer.Write(attrVal.AsBool());
                    break;

                case AttrTypeTag.CkUint:
                    writer.Write(attrVal.AsUint());
                    break;

                case AttrTypeTag.DateTime:
                    writer.Write(attrVal.AsDate().ToString());
                    break;

                case AttrTypeTag.String:
                    writer.Write(attrVal.AsString());
                    break;

                default:
                    throw new InvalidProgramException($"Enum value {attrVal.TypeTag} is not supported.");
            }
        }
    }

    public StorageObjectMemento Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        options.Security.DepthStep(ref reader);

        StorageObjectMemento memento = new StorageObjectMemento();

        if (reader.ReadArrayHeader() != 3)
        {
            throw new InvalidDataException("Invalid array header in storage memento object.");
        }

        if (reader.ReadInt32() != 1)
        {
            throw new InvalidDataException("Invalid version in storage memento object.");
        }

        memento.Id = new Guid(this.ReadByteArray(ref reader));

        int attributesCount = reader.ReadMapHeader();
        for (int i = 0; i < attributesCount; i++)
        {
            if (reader.ReadArrayHeader() != 3)
            {
                throw new InvalidDataException("Invalid array header in attribute in storage memento object.");
            }

            CKA attrType = (CKA)reader.ReadUInt32();
            AttrTypeTag typeTag = (AttrTypeTag)reader.ReadUInt32();
            IAttributeValue attrVal = typeTag switch
            {
                AttrTypeTag.ByteArray => AttributeValue.Create(this.ReadByteArray(ref reader)),
                AttrTypeTag.CkAttributeArray => throw new NotImplementedException(),
                AttrTypeTag.CkBool => AttributeValue.Create(reader.ReadBoolean()),
                AttrTypeTag.CkUint => AttributeValue.Create(reader.ReadUInt32()),
                AttrTypeTag.DateTime => AttributeValue.Create(CkDate.Parse(reader.ReadString())),
                AttrTypeTag.String => AttributeValue.Create(reader.ReadString() ?? string.Empty),
                _ => throw new InvalidProgramException($"Enum value {typeTag} is not supported.")
            };

            memento.Values.Add(attrType, attrVal);
        }

        return memento;
    }

    private byte[] ReadByteArray(ref MessagePackReader reader)
    {
        ReadOnlySequence<byte>? idBytes = reader.ReadBytes();
        if (!idBytes.HasValue)
        {
            throw new InvalidDataException("Can not read byte array.");
        }

        return idBytes.Value.ToArray();
    }
}