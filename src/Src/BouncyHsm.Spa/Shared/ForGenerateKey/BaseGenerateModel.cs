﻿using BouncyHsm.Client;
using System.ComponentModel.DataAnnotations;

namespace BouncyHsm.Spa.Shared.ForGenerateKey;

internal abstract class BaseGenerateModel
{
    [Required]
    [MaxLength(1024)]
    public string CkaLabel
    {
        get;
        set;
    }

    public string CkaIdText
    {
        get;
        set;
    }

    public BinaryForm CkaIdForm
    {
        get;
        set;
    }

    [Required]
    public bool Exportable
    {
        get;
        set;
    }

    [Required]
    public bool Sensitive
    {
        get;
        set;
    }

    [Required]
    public bool ForSigning
    {
        get;
        set;
    }

    [Required]
    public bool ForEncryption
    {
        get;
        set;
    }

    [Required]
    public bool ForDerivation
    {
        get;
        set;
    }

    [Required]
    public bool ForWrap
    {
        get;
        set;
    }

    public BaseGenerateModel()
    {
        this.CkaLabel = string.Empty;
        this.CkaIdText = string.Empty;
        this.ForDerivation = false;
    }

    internal GenerateKeyAttributesDto ToGenerateKeyAttributesDto()
    {
        string ckIdText = this.CkaIdText.Trim();
        return new GenerateKeyAttributesDto()
        {
            CkaId = string.IsNullOrEmpty(ckIdText) ? null : this.CkaIdForm.GetCkaId(ckIdText),
            CkaLabel = this.CkaLabel.Trim(),
            Exportable = this.Exportable,
            ForDerivation = this.ForDerivation,
            ForEncryption = this.ForEncryption,
            ForSigning = this.ForSigning,
            ForWrap = this.ForWrap,
            Sensitive = this.Sensitive
        };
    }

}
