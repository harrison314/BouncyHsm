﻿namespace BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;

public class SlotModel
{
    public Guid Id
    {
        get;
        set;
    }

    public uint SlotId
    {
        get;
        set;
    }

    public bool IsHwDevice
    {
        get;
        set;
    }

    public bool IsRemovableDevice
    {
        get;
        set;
    }

    public bool IsUnplugged
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public TokenInfoModel Token
    {
        get;
        set;
    }

    public DateTime Created
    {
        get;
        set;
    }

    public SlotModel()
    {
        this.Description = string.Empty;
        this.Token = new TokenInfoModel();
    }
}
