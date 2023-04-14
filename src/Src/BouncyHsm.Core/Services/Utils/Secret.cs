using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Utils;

[TypeConverter(typeof(ConfigurationSecretTypeConverter))]
public sealed class Secret
{
    private readonly ReadOnlyMemory<char> data;

    public Secret(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("value is empty string.", nameof(value));

        char[] buffer = GC.AllocateUninitializedArray<char>(value.Length, pinned: true);
        value.AsSpan().CopyTo(buffer);
        this.data = buffer;
    }

    public string Reveal()
    {
        return string.Create(this.data.Length, this.data, (span, data) => data.Span.CopyTo(span));
    }

    [Obsolete($"Use {nameof(Reveal)} instead")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
    public override string? ToString()
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
    {
        return base.ToString();
    }

    private sealed class ConfigurationSecretTypeConverter : TypeConverter
    {
        public ConfigurationSecretTypeConverter()
        {

        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
        {
            return false;
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            throw new InvalidOperationException();
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
        {
            return value is string { Length: > 0 } str ? new Secret(str) : null;
        }
    }
}