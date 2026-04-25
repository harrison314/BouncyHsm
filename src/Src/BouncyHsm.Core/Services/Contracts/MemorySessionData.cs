namespace BouncyHsm.Core.Services.Contracts;

public class MemorySessionData
{
    public string ComputerName
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

    public string[] CmdLine
    {
        get;
        private set;
    }

    public MemorySessionData(string computerName, string applicationName, ulong pid, uint ptrSize, uint ckUlongSize, string[] cmdLine)
    {
        this.ComputerName = computerName;
        this.ApplicationName = applicationName;
        this.Pid = pid;
        this.PtrSize = ptrSize;
        this.CkUlongSize = ckUlongSize;
        this.CmdLine = cmdLine;
    }

    public override string ToString()
    {
        return $"MemorySession: on {this.ComputerName}, Application: {this.ApplicationName} ({this.Pid})";
    }
}
