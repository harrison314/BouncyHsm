using BouncyHsm.Core.Services.Contracts.P11;
using MessagePack;
using MessagePack.Formatters;
using System.Buffers;

namespace BouncyHsm.Core.Services.Contracts.Entities;

internal class StorageObjectMementoMessagePackFormatter : IMessagePackFormatter<StorageObjectMemento?>
{
    public StorageObjectMementoMessagePackFormatter()
    {

    }

    public void Serialize(ref MessagePackWriter writer, StorageObjectMemento? value, MessagePackSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNil();
            return;
        }

        writer.WriteArrayHeader(3);
        writer.Write(1); // Version
        writer.Write(value.Id.ToByteArray());
        this.WriteValues(ref writer, value.Values);
    }

    private void WriteValues(ref MessagePackWriter writer, IReadOnlyDictionary<CKA, IAttributeValue> values)
    {
        writer.WriteMapHeader(values.Count);

        foreach ((CKA attrType, IAttributeValue attrVal) in values)
        {
            writer.WriteArrayHeader(3);
            writer.Write((uint)attrType);
            writer.Write((uint)attrVal.TypeTag);

            switch (attrVal.TypeTag)
            {
                case AttrTypeTag.ByteArray:
                    writer.Write(attrVal.AsByteArray());
                    break;

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

                case AttrTypeTag.UintArray:
                    this.WriteUintArray(ref writer, attrVal);
                    break;

                case AttrTypeTag.CkAttributeArray:
                    this.WriteTemplate(ref writer, attrVal);
                    break;

                default:
                    throw new InvalidProgramException($"Enum value {attrVal.TypeTag} is not supported.");
            }
        }
    }

    public StorageObjectMemento? Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
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
        this.ReadValues(ref reader, memento.Values);

        return memento;
    }

    private void ReadValues(ref MessagePackReader reader, Dictionary<CKA, IAttributeValue> values)
    {
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
                AttrTypeTag.CkBool => AttributeValue.Create(reader.ReadBoolean()),
                AttrTypeTag.CkUint => AttributeValue.Create(reader.ReadUInt32()),
                AttrTypeTag.DateTime => AttributeValue.Create(CkDate.Parse(reader.ReadString())),
                AttrTypeTag.String => AttributeValue.Create(reader.ReadString() ?? string.Empty),
                AttrTypeTag.UintArray => AttributeValue.Create(this.ReadUintArray(ref reader)),
                AttrTypeTag.CkAttributeArray => this.ReadTemplate(ref reader),
                _ => throw new InvalidProgramException($"Enum value {typeTag} is not supported.")
            };

            values.Add(attrType, attrVal);
        }
    }

    private byte[] ReadByteArray(ref MessagePackReader reader)
    {
        ReadOnlySequence<byte>? idBytes = reader.ReadBytes();
        if (!idBytes.HasValue)
        {
            throw new InvalidDataException("Can not read byte array.");
        }

        if (idBytes.Value.Length == 0)
        {
            return Array.Empty<byte>();
        }

        return idBytes.Value.ToArray();
    }

    private void WriteUintArray(ref MessagePackWriter writer, IAttributeValue attrVal)
    {
        uint[] uintArray = attrVal.AsUintArray();
        writer.WriteArrayHeader(uintArray.Length);
        for (int i = 0; i < uintArray.Length; i++)
        {
            writer.Write(uintArray[i]);
        }
    }

    private uint[] ReadUintArray(ref MessagePackReader reader)
    {
        int header = reader.ReadArrayHeader();
        if (header == 0)
        {
            return Array.Empty<uint>();
        }

        uint[] array = new uint[header];
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = reader.ReadUInt32();
        }

        return array;
    }

    private void WriteTemplate(ref MessagePackWriter writer, IAttributeValue attrVal)
    {
        this.WriteValues(ref writer, attrVal.AsCkAttributeArray());
    }

    private IAttributeValue ReadTemplate(ref MessagePackReader reader)
    {
        Dictionary<CKA, IAttributeValue> values = new Dictionary<CKA, IAttributeValue>();
        this.ReadValues(ref reader, values);

        return AttributeValue.Create(values);
    }
}