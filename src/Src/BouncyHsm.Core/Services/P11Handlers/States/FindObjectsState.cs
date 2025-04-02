using BouncyHsm.Core.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.States;

internal class FindObjectsState : ISessionState
{
    private readonly List<uint> handlers;
    private int start;

    public int AvailableObjects
    {
        get => this.handlers.Count - this.start;
    }

    public FindObjectsState(List<uint> handlers)
    {
        System.Diagnostics.Debug.Assert(handlers != null);

        this.handlers = handlers;
        this.start = 0;
    }

    public uint[] PullObjects(uint count)
    {
        //TODO: optimize
        uint[] pulledHandlers = this.handlers.Skip(this.start)
            .Take(Convert.ToInt32(count))
            .ToArray();

        this.start += pulledHandlers.Length;

        return pulledHandlers;
    }

    public override string ToString()
    {
        return $"Find objects state - found: {this.handlers.Count} read: {this.start}";
    }
}
