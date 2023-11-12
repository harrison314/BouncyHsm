namespace BouncyHsm.Core.Services.Contracts;

public class MemorySessionData
{
    public string CompiuterName
    {
        get;
        private set;
    }

    public string ApplicationName
    {
        get;
        private set;
    }

    public ulong Pid
    {
        get;
        private set;
    }

    public uint PtrSize
    {
        get;
        private set;
    }

    public uint CkUlongSize
    {
        get;
        private set;
    }

    public MemorySessionData(string compiuterName, string applicationName, ulong pid, uint ptrSize, uint ckUlongSize)
    {
        this.CompiuterName = compiuterName;
        this.ApplicationName = applicationName;
        this.Pid = pid;
        this.PtrSize = ptrSize;
        this.CkUlongSize = ckUlongSize;
    }

    public override string ToString()
    {
        return $"MemorySession: on {this.CompiuterName}, Application: {this.ApplicationName} ({this.Pid})";
    }
}
