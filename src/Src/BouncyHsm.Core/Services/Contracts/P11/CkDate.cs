using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.P11;

public struct CkDate : IEquatable<CkDate>
{
    private readonly DateOnly value;

    public bool HasValue
    {
        get;
    }

    public DateOnly Value
    {
        get
        {
            if (!this.HasValue)
            {
                throw new InvalidOperationException("CkDate is empty");
            }

            return this.value;
        }
    }

    public CkDate()
    {
        this.HasValue = false;
        this.value = new DateOnly();
    }

    public CkDate(DateOnly date)
    {
        this.HasValue = true;
        this.value = date;
    }

    public CkDate(DateTime date)
    {
        this.HasValue = true;
        this.value = DateOnly.FromDateTime(date);
    }

    public static CkDate Parse(ReadOnlySpan<char> date)
    {
        if (date.Length == 0)
        {
            return new CkDate();
        }

        DateOnly value = DateOnly.ParseExact(date, "dd.MM.yyyy");
        return new CkDate(value);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is CkDate date)
        {
            return this.Equals(other: date);
        }

        return false;
    }

    public bool Equals(CkDate other)
    {
        if (this.HasValue != other.HasValue)
        {
            return false;
        }

        if (this.HasValue)
        {
            return this.value.Equals(other.value);
        }
        else
        {
            return true;
        }
    }

    public override int GetHashCode()
    {
        if (this.HasValue)
        {
            return this.value.GetHashCode();
        }
        else
        {
            return 0;
        }
    }

    public override string ToString()
    {
        if (this.HasValue)
        {
            return string.Format("{0:dd}.{0:MM}.{0:yyyy}", this.value);
        }
        else
        {
            return string.Empty;
        }
    }

    internal string? ToRpcString()
    {
        if (this.HasValue)
        {
            return string.Format("{0:dd}.{0:MM}.{0:yyyy}", this.value);
        }
        else
        {
            return null;
        }
    }
}
