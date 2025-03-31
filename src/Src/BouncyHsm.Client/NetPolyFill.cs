using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETSTANDARD2_1
namespace System.Runtime.Versioning
{
    internal abstract class OSPlatformAttribute : Attribute
    {
        public string PlatformName { get; }
        private protected OSPlatformAttribute(string platformName)
        {
            this.PlatformName = platformName;
        }
    }

    [AttributeUsage(AttributeTargets.Assembly |
                    AttributeTargets.Class |
                    AttributeTargets.Constructor |
                    AttributeTargets.Enum |
                    AttributeTargets.Event |
                    AttributeTargets.Field |
                    AttributeTargets.Interface |
                    AttributeTargets.Method |
                    AttributeTargets.Module |
                    AttributeTargets.Property |
                    AttributeTargets.Struct,
                    AllowMultiple = true, Inherited = false)]
    internal sealed class UnsupportedOSPlatformAttribute : OSPlatformAttribute
    {
        public string? Message { get; }

        public UnsupportedOSPlatformAttribute(string platformName) : base(platformName)
        {
        }

        public UnsupportedOSPlatformAttribute(string platformName, string? message) : base(platformName)
        {
            this.Message = message;
        }
    }
}

#endif