using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BouncyHsm.RpcGenerator.Generators.CSharp;

internal class CSharpDeclaredType : DeclaredType
{
    public string CharpType
    {
        get;
    }

    public CSharpDeclaredType(string def) : base(def)
    {
        this.CharpType = this.TranslateType();
    }

    private string TranslateType()
    {
        string cBaseType = this.TranslateBaseType();
        if (this.IsArray)
        {
            return (this.IsNullable) ? $"{cBaseType}[]?" : $"{cBaseType}[]";
        }

        return (this.IsNullable) ? $"{cBaseType}?" : cBaseType;
    }

    private string TranslateBaseType()
    {
        return this.BaseDefinition switch
        {
            DeclaredType.BinaryName => "byte[]",
            DeclaredType.StringName => "string",
            DeclaredType.Int64Name => "long",
            DeclaredType.BoolName => "bool",
            DeclaredType.DoubleName => "double",
            DeclaredType.Int32Name => "int",
            DeclaredType.UInt32Name => "uint",
            DeclaredType.UInt64Name => "ulong",
            _ => this.BaseDefinition
        };
    }
}
