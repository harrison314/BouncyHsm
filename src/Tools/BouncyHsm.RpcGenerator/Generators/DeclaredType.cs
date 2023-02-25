using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.RpcGenerator.Generators;

internal class DeclaredType
{
    public const string StringName = "string";

    public const string Int32Name = "int32";
    public const string UInt32Name = "uint32";
    public const string Int64Name = "int64";
    public const string UInt64Name = "uint64";
    public const string DoubleName = "double";

    public const string BoolName = "bool";

    public const string BinaryName = "binary";

    public string OriginalDefinition
    {
        get;
    }

    public string BaseDefinition
    {
        get;
    }

    public bool IsNullable
    {
        get;
    }

    public bool IsArray
    {
        get;
    }

    public bool IsBaseType
    {
        get;
    }

    public bool IsNumericType
    {
        get => this.BaseDefinition is Int32Name
            or UInt32Name
            or Int64Name
            or UInt64Name
            or DoubleName;
    }

    public DeclaredType(string def)
    {
        this.OriginalDefinition = def;
        this.BaseDefinition = this.OriginalDefinition;
        if (this.BaseDefinition.EndsWith('?'))
        {
            this.BaseDefinition = def[..^1];
            this.IsNullable = true;
        }

        if (this.BaseDefinition.EndsWith("[]"))
        {
            this.BaseDefinition = this.BaseDefinition[..^2];
            this.IsArray = true;
        }

        this.IsBaseType = this.BaseDefinition is Int32Name
            or UInt32Name
            or Int64Name
            or UInt64Name
            or DoubleName
            or StringName
            or BinaryName
            or BoolName;
    }

    public override bool Equals(object? obj)
    {
        DeclaredType? other = obj as DeclaredType;
        if (other == null) return false;

        return this.OriginalDefinition == other.OriginalDefinition;
    }

    public override int GetHashCode()
    {
        return this.OriginalDefinition.GetHashCode();
    }
}
