using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BouncyHsm.RpcGenerator.Generators.C;

internal class CDeclaredType : DeclaredType
{
    public string CType
    {
        get;
        private set;
    }

    public CDeclaredType(string def)
        : base(def)
    {
        this.CType = this.GetCType();
    }

    public string GetTypeFromAray()
    {
        return this.TranslateBaseType();
    }

    private string GetCType()
    {
        string cBaseType = this.TranslateBaseType();
        if (this.IsArray)
        {
            if (this.BaseDefinition == StringName)
            {
                cBaseType = "String";
            }

            return (this.IsNullable) ? $"ArrayOf{cBaseType}*" : $"ArrayOf{cBaseType}";
        }

        if (this.BaseDefinition == StringName)
        {
            return cBaseType;
        }

        return (this.IsNullable) ? $"{cBaseType}*" : cBaseType;
    }

    public override bool Equals(object? obj)
    {
        if (obj is CDeclaredType other)
        {
            return this.CType == other.CType;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return this.CType.GetHashCode();
    }

    public override string ToString()
    {
        return $"C type: {this.CType}";
    }

    private string TranslateBaseType()
    {
        return this.BaseDefinition switch
        {
            DeclaredType.BinaryName => "Binary",
            DeclaredType.StringName => "const char*",
            DeclaredType.Int64Name => "int64_t",
            DeclaredType.BoolName => "bool",
            DeclaredType.DoubleName => "double",
            DeclaredType.Int32Name => "int32_t",
            DeclaredType.UInt32Name => "uint32_t",
            DeclaredType.UInt64Name => "uint64_t",
            _ => this.BaseDefinition
        };
    }
}
