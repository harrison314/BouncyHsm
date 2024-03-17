using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile;

internal class LiteDbPersistentRepository : IPersistentRepository, IDisposable
{
    private readonly LiteDatabase database;
    private readonly ILogger<LiteDbPersistentRepository> logger;
    private bool disposedValue;

    public LiteDbPersistentRepository(IOptions<LiteDbPersistentRepositorySetup> persistenceSetup,
        ILogger<LiteDbPersistentRepository> logger)
    {
        ConnectionString connectionString = new ConnectionString()
        {
            Collation = Collation.Binary,
            Connection = ConnectionType.Direct,
            InitialSize = 32 * 8192,
            Upgrade = false,
            Filename = persistenceSetup.Value.DbFilePath,
            Password = null,
            ReadOnly = persistenceSetup.Value.ReadOnly
        };

        this.database = new LiteDatabase(connectionString);
        this.database.UtcDate = true;

        if (persistenceSetup.Value.ReduceLogFileSize)
        {
            this.database.CheckpointSize = 100;
        }

        logger.LogInformation("Open database {databasePath}.", connectionString.Filename);

        this.logger = logger;

        this.InitAllIndex();
        this.InitVersionRecord();
    }

    #region Slot Management

    public ValueTask<SlotIds> CreateSlot(SlotEntity slot, TokenPins? pins, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering CreateSlot with description {Description}.", slot.Description);

        ILiteCollection<SlotModel> collection = this.database.GetCollection<SlotModel>();
        ILiteCollection<PasswordsModel> passwordCollection = this.database.GetCollection<PasswordsModel>();
        ILiteCollection<SlotSequence> slotSequence = this.database.GetCollection<SlotSequence>();

        if (pins == null) throw new ArgumentNullException(nameof(pins));
        if (slot == null) throw new ArgumentNullException(nameof(slot));

        SlotMapper mapper = new SlotMapper();
        SlotModel slotModel = mapper.MapSlot(slot);

        int insertedId = slotSequence.Insert(new SlotSequence());
        this.logger.LogDebug("Create new slotId using slot sequence {insertedId}.", insertedId);

        slotModel.Id = Guid.NewGuid();
        slotModel.SlotId = (uint)insertedId;
        slotModel.Created = DateTime.UtcNow;

        PasswordsModel passwords = new PasswordsModel()
        {
            Id = slotModel.Id,
            UserPin = this.HashPassword(pins.UserPin),
            SoPin = this.HashPassword(pins.SoPin),
            SignaturePin = (pins.SignaturePin != null) ? this.HashPassword(pins.SignaturePin) : null
        };

        this.database.ExecuteInTransaction(_ =>
        {
            try
            {
                passwordCollection.Insert(passwords);
                this.logger.LogDebug("Inserted passwords {Id}.", passwords.Id);
            }
            catch (LiteException ex) when (ex.Message.Contains("IX_SlotEntity_TokenSerial"))
            {
                this.logger.LogError(ex, "Token serial {TokenSerial} already exists.", slot.Token.SerialNumber);
                throw new BouncyHsmStorageException($"Token serial {slot.Token.SerialNumber} already exists.", ex);
            }

            collection.Insert(slotModel);
            this.logger.LogDebug("Inserted slot model {Id}.", slotModel.Id);
        });

        return new ValueTask<SlotIds>(new SlotIds(slotModel.Id, slotModel.SlotId));
    }

    private Pbkdf2PasswordModel HashPassword(string password)
    {
        this.logger.LogTrace("Entering to HashPassword.");

        const int iterations = 350_000;

        Pbkdf2PasswordModel model = new Pbkdf2PasswordModel();
        model.Iterations = iterations;
        model.Salt = RandomNumberGenerator.GetBytes(32);
        model.Hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            model.Salt,
            iterations,
            HashAlgorithmName.SHA256,
            64);

        return model;
    }

    private bool VerifyHashPassword(Pbkdf2PasswordModel hashModel, string password)
    {
        this.logger.LogTrace("Entering to VerifyHashPassword.");

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            hashModel.Salt,
            hashModel.Iterations,
            HashAlgorithmName.SHA256,
            hashModel.Hash.Length);

        return CryptographicOperations.FixedTimeEquals(hash, hashModel.Hash);
    }


    public ValueTask<SlotEntity?> GetSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlot with slotId {slotId}.", slotId);

        ILiteCollection<SlotModel> collection = this.database.GetCollection<SlotModel>();
        SlotModel? model = collection.FindOne(t => t.SlotId == slotId);

        if (model == null)
        {
            return new ValueTask<SlotEntity?>(null as SlotEntity);
        }

        SlotMapper mapper = new SlotMapper();

        return new ValueTask<SlotEntity?>(mapper.MapSlot(model));
    }

    public ValueTask<IReadOnlyList<SlotEntity>> GetSlots(GetSlotSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetSlots with WithTokenPresent {WithTokenPresent}.", specification.WithTokenPresent);

        ILiteCollection<SlotModel> collection = this.database.GetCollection<SlotModel>();
        SlotMapper mapper = new SlotMapper();

        IReadOnlyList<SlotEntity> list = collection.FindAll().Select(t => mapper.MapSlot(t)).ToList();
        return new ValueTask<IReadOnlyList<SlotEntity>>(list);
    }

    public ValueTask DeleteSlot(uint slotId, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DeleteSlot with slotId {slotId}.", slotId);

        ILiteCollection<SlotModel> collection = this.database.GetCollection<SlotModel>();
        int deletedSlots = collection.DeleteMany(t => t.SlotId == slotId);
        if (deletedSlots != 1)
        {
            throw new BouncyHsmNotFoundException("Slot with slotId {slotId} not found.");
        }

        this.logger.LogDebug("Slot with slotId {slotId} has removed from database.", slotId);

        ILiteCollection<StorageObjectInfo> objectCollection = this.database.GetCollection<StorageObjectInfo>();

        const int pageSize = 100;
        List<Guid> objectsIds = new List<Guid>(pageSize);
        for (; ; )
        {
            objectsIds.Clear();
            IEnumerable<Guid> ids = objectCollection.Find(t => t.SlotId == slotId, 0, pageSize).Select(t => t.Id);
            objectsIds.AddRange(ids);

            if (objectsIds.Count == 0)
            {
                break;
            }

            foreach (Guid objectId in objectsIds)
            {
                this.database.ExecuteInTransaction(db =>
                {
                    objectCollection.Delete(objectId);
                    this.database.GetStorage<Guid>().Delete(objectId);
                });

                this.logger.LogDebug("Removed object with id {objectId}.", objectId);
            }
        }

        return new ValueTask();
    }

    public ValueTask<bool> ValidatePin(SlotEntity slot, CKU userType, string pin, object? context, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to ValidatePin with slotId {slotId}, userType {userType}.", slot.SlotId, userType);

        ILiteCollection<PasswordsModel> passwordCollection = this.database.GetCollection<PasswordsModel>();

        PasswordsModel model = passwordCollection.FindById(slot.Id);
        Pbkdf2PasswordModel? hashModel = userType switch
        {
            CKU.CKU_USER => model.UserPin,
            CKU.CKU_SO => model.SoPin,
            CKU.CKU_CONTEXT_SPECIFIC => model.SignaturePin,
            _ => throw new InvalidProgramException($"Enum value {userType} is not supported.")
        };

        if (hashModel == null)
        {
            return new ValueTask<bool>(true);
        }

        return new ValueTask<bool>(this.VerifyHashPassword(hashModel, pin));
    }

    #endregion


    #region Object management

    public ValueTask StoreObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to StoreObject with slotId {slotId}, storageObject {storageObject}.", slotId, storageObject);

        if (storageObject.Id == Guid.Empty)
        {
            storageObject.Id = Guid.NewGuid();
        }

        ILiteCollection<StorageObjectInfo> collection = this.database.GetCollection<StorageObjectInfo>();

        (StorageObjectInfo info, StorageObjectMemento memento) = this.BuildMemento(slotId, storageObject);

        this.database.ExecuteInTransaction(_ =>
        {
            using (MemoryStream ms = new MemoryStream(memento.ToByteArray(), false))
            {
                this.database.GetStorage<Guid>().Upload(memento.Id,
                    $"{memento.Id:D}.dat",
                    ms);
            }

            collection.Insert(info);
        });

        this.database.Checkpoint();

        return new ValueTask();
    }

    public ValueTask UpdateObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to UpdateObject with slotId {slotId}, storageObject {storageObject}.", slotId, storageObject);

        ILiteCollection<StorageObjectInfo> collection = this.database.GetCollection<StorageObjectInfo>();
        (StorageObjectInfo info, StorageObjectMemento memento) = this.BuildMemento(slotId, storageObject);

        this.database.ExecuteInTransaction(_ =>
        {
            if (!collection.Delete(storageObject.Id))
            {
                throw new BouncyHsmStorageException($"Not found object with id {storageObject.Id}");
            }

            if (!this.database.GetStorage<Guid>().Delete(storageObject.Id))
            {
                throw new BouncyHsmStorageException($"Not found object with id {storageObject.Id}");
            }

            using (MemoryStream ms = new MemoryStream(memento.ToByteArray(), false))
            {
                this.database.GetStorage<Guid>().Upload(memento.Id,
                    $"{memento.Id:D}.dat",
                    ms);
            }

            collection.Insert(info);
        });

        this.database.Checkpoint();

        return new ValueTask();
    }

    public ValueTask DestroyObject(uint slotId, StorageObject storageObject, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to DestroyObject with slotId {slotId}, objectId {objectId}.", slotId, storageObject.Id);

        ILiteCollection<StorageObjectInfo> collection = this.database.GetCollection<StorageObjectInfo>();

        this.database.ExecuteInTransaction(_ =>
        {
            if (!collection.Delete(storageObject.Id))
            {
                throw new BouncyHsmStorageException($"Not found object with id {storageObject.Id}");
            }

            if (!this.database.GetStorage<Guid>().Delete(storageObject.Id))
            {
                throw new BouncyHsmStorageException($"Not found object with id {storageObject.Id}");
            }
        });

        this.database.Checkpoint();

        return new ValueTask();
    }

    public ValueTask<StorageObject?> TryLoadObject(uint slotId, Guid id, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to TryLoadObject with slotId {slotId}, objectId {objectId}.", slotId, id);

        ILiteCollection<StorageObjectInfo> collection = this.database.GetCollection<StorageObjectInfo>();

        StorageObjectInfo? info = collection.FindOne(t => t.Id == id);
        if (info == null)
        {
            return new ValueTask<StorageObject?>(null as StorageObject);
        }

        if (info.SlotId != slotId)
        {
            throw new InvalidProgramException("Mishmash slotId in load object.");
        }

        using MemoryStream ms = new MemoryStream();
        LiteFileInfo<Guid> lfInfo = this.database.GetStorage<Guid>().Download(id, ms);

        StorageObjectMemento memento = StorageObjectMemento.FromInstance(ms.ToArray());
        StorageObject storageObject = StorageObjectFactory.CreateFromMemento(memento);

        return new ValueTask<StorageObject?>(storageObject);
    }

    public ValueTask<IReadOnlyList<StorageObject>> FindObjects(uint slotId, FindObjectSpecification specification, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to FindObjects with slotId {slotId}.", slotId);

        ILiteCollection<StorageObjectInfo> collection = this.database.GetCollection<StorageObjectInfo>();
        BsonExpression query = this.BuildQuery(slotId, specification);
        List<Guid> ids = collection.Find(query).Select(t => t.Id).ToList();

        List<StorageObject> result = new List<StorageObject>();
        foreach (Guid id in ids)
        {
            using MemoryStream ms = new MemoryStream();
            LiteFileInfo<Guid> lfInfo = this.database.GetStorage<Guid>().Download(id, ms);

            StorageObjectMemento memento = StorageObjectMemento.FromInstance(ms.ToArray());
            StorageObject storageObject = StorageObjectFactory.CreateFromMemento(memento);

            if (storageObject.IsMatch(specification.Template))
            {
                result.Add(storageObject);
            }
        }

        return new ValueTask<IReadOnlyList<StorageObject>>(result);
    }

    private (StorageObjectInfo info, StorageObjectMemento memento) BuildMemento(uint slotId, StorageObject storageObject)
    {
        StorageObjectInfo info = new StorageObjectInfo()
        {
            Id = storageObject.Id,
            CkaClass = (uint)storageObject.CkaClass,
            LabelHash = this.CalculateXxxHash(storageObject.CkaLabel),
            IsPrivate = storageObject.CkaPrivate,
            SlotId = slotId,
            Created = DateTime.UtcNow
        };

        StorageObjectMemento memento = storageObject.ToMemento();
        if (memento.Values.TryGetValue(CKA.CKA_CERTIFICATE_TYPE, out IAttributeValue? attributeValue))
        {
            info.CertType = attributeValue.AsUint();
        }

        if (memento.Values.TryGetValue(CKA.CKA_KEY_TYPE, out IAttributeValue? attrKeyValue))
        {
            info.KeyType = attrKeyValue.AsUint();
        }

        if (memento.Values.TryGetValue(CKA.CKA_ID, out IAttributeValue? idValue))
        {
            info.IdHash = this.CalculateXxxHash(idValue.AsByteArray());
        }

        return (info, memento);
    }

    private BsonExpression BuildQuery(uint slotId, FindObjectSpecification specification)
    {
        this.logger.LogTrace("Entering to BuildQuery with slotId {slotId}.", slotId);

        List<BsonExpression> expressions = new List<BsonExpression>();

        byte[]? id = specification.TryGetBytesValue(CKA.CKA_ID);
        if (id != null)
        {
            ulong idHash = this.CalculateXxxHash(id);
            expressions.Add(Query.EQ(nameof(StorageObjectInfo.IdHash), (long)idHash));
        }

        string? label = specification.TryGetStringValue(CKA.CKA_LABEL);
        if (label != null)
        {
            ulong labelHash = this.CalculateXxxHash(label);
            expressions.Add(Query.EQ(nameof(StorageObjectInfo.LabelHash), (long)labelHash));
        }

        uint? classValue = specification.TryGetUintValue(CKA.CKA_CLASS);
        if (classValue.HasValue)
        {
            expressions.Add(Query.EQ(nameof(StorageObjectInfo.CkaClass), (long)classValue.Value));
        }

        expressions.Add(Query.EQ(nameof(StorageObjectInfo.SlotId), (long)slotId));

        if (!specification.IsUserLogged)
        {
            expressions.Add(Query.EQ(nameof(StorageObjectInfo.IsPrivate), false));
        }

        if (expressions.Count == 1)
        {
            return expressions[0];
        }

        return Query.And(queries: expressions.ToArray());
    }

    private void InitAllIndex()
    {
        this.logger.LogTrace("Entering to InitAllIndex");

        ILiteCollection<StorageObjectInfo> collection = this.database.GetCollection<StorageObjectInfo>();
        if (collection.EnsureIndex("IX_StorageObjectInfo_LabelHash", t => t.LabelHash))
        {
            this.logger.LogDebug("Init index {indexName}.", "IX_StorageObjectInfo_LabelHash");
        }

        if (collection.EnsureIndex("IX_StorageObjectInfo_IdHash", t => t.IdHash))
        {
            this.logger.LogDebug("Init index {indexName}.", "IX_StorageObjectInfo_IdHash");
        }

        if (collection.EnsureIndex("IX_StorageObjectInfo_CkaClass", t => t.CkaClass))
        {
            this.logger.LogDebug("Init index {indexName}.", "IX_StorageObjectInfo_CkaClass");
        }

        if (collection.EnsureIndex("IX_StorageObjectInfo_SlotId", t => t.SlotId))
        {
            this.logger.LogDebug("Init index {indexName}.", "IX_StorageObjectInfo_SlotId");
        }

        ILiteCollection<SlotModel> slotCollection = this.database.GetCollection<SlotModel>();
        if (slotCollection.EnsureIndex("IX_SlotEntity_TokenSerial", t => t.Token.SerialNumber, true))
        {
            this.logger.LogDebug("Init index {indexName}.", "IX_SlotEntity_TokenSerial");
        }
    }

    private void InitVersionRecord()
    {
        this.logger.LogTrace("Entering to InitVersionRecord");

        string? version = this.GetType().Assembly.GetName().Version?.ToString();
        System.Diagnostics.Debug.Assert(version != null);

        ILiteCollection<VersionModel> collection = this.database.GetCollection<VersionModel>();
        // TODO: Check migrations

        VersionModel? currentVersion = collection.FindById(version);
        if (currentVersion == null)
        {
            currentVersion = new VersionModel()
            {
                Id = version,
                MigrationTime = DateTime.UtcNow
            };

            collection.Insert(currentVersion);
            this.logger.LogInformation("Insert new version recored with version {version}.", version);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                try
                {
                    this.database.Checkpoint();
                    this.database.Dispose();
                    this.logger.LogDebug("Close database.");
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Error during close database.");
                }
            }

            this.disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    public ValueTask<PersistentRepositoryStats> GetStats(CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to GetStats");

        ILiteCollection<SlotModel> collection = this.database.GetCollection<SlotModel>();
        ILiteCollection<StorageObjectInfo> objectCollection = this.database.GetCollection<StorageObjectInfo>();

        int privateKeys = objectCollection.Count(t => t.CkaClass == (uint)CKO.CKO_PRIVATE_KEY);
        int certificates = objectCollection.Count(t => t.CkaClass == (uint)CKO.CKO_CERTIFICATE
            && t.CertType.HasValue
            && t.CertType.Value == (uint)CKC.CKC_X_509);

        return new ValueTask<PersistentRepositoryStats>(new PersistentRepositoryStats(collection.Count(),
           objectCollection.Count(),
           privateKeys,
           certificates));
    }

    private ulong CalculateXxxHash(ReadOnlySpan<byte> data)
    {
        this.logger.LogTrace("Entering to CalculateXxxHash.");

        Span<byte> hashValue = stackalloc byte[8];
        System.IO.Hashing.XxHash64.Hash(data, hashValue, 0);

        return BitConverter.ToUInt64(hashValue);
    }

    private ulong CalculateXxxHash(ReadOnlySpan<char> data)
    {
        this.logger.LogTrace("Entering to CalculateXxxHash.");

        Span<byte> hashValue = stackalloc byte[8];
        int encodedSize = Encoding.UTF8.GetByteCount(data);
        Span<byte> encodedData = (encodedSize <= 512) ? stackalloc byte[encodedSize] : new byte[encodedSize];

        System.IO.Hashing.XxHash64.Hash(encodedData, hashValue, 0);

        return BitConverter.ToUInt64(hashValue);
    }
}
