using System.ComponentModel;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<NetRuntime>))]
public class NetRuntime : Enumeration
{
    public static NetRuntime None = new NetRuntime() { Value = string.Empty };

    public static NetRuntime win_x64 = new NetRuntime() { Value = "win-x64" };
    public static NetRuntime win_x86 = new NetRuntime() { Value = "win-x86" };
    public static NetRuntime win_arm64 = new NetRuntime() { Value = "win-arm64" };

    public static NetRuntime win10_x64 = new NetRuntime() { Value = "win10-x64" };
    public static NetRuntime win10_x86 = new NetRuntime() { Value = "win10-x86" };
    public static NetRuntime win10_arm64 = new NetRuntime() { Value = "win10-arm64" };

    public static NetRuntime linux_x64 = new NetRuntime() { Value = "linux-x64" };
    public static NetRuntime linux_musl_x64 = new NetRuntime() { Value = "linux-musl-x64" };
    public static NetRuntime linux_arm = new NetRuntime() { Value = "linux-arm" };
    public static NetRuntime linux_arm64 = new NetRuntime() { Value = "linux-arm64" };

    public NetRuntime()
    {

    }

    public static implicit operator string(NetRuntime configuration)
    {
        return configuration.Value;
    }
}
