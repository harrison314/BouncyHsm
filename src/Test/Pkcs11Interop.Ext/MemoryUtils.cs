using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Pkcs11Interop.Ext;

internal static unsafe class MemoryUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntPtr MemDup(byte[] data)
    {
        System.Diagnostics.Debug.Assert(data != null);

        void* ptr = NativeMemory.Alloc((nuint)data.Length);
        System.Diagnostics.Debug.Assert(ptr != null, "Memory allocation failed");

        Unsafe.CopyBlock(ref Unsafe.AsRef<byte>(ptr), ref data[0], (uint)data.Length);
        return (IntPtr)ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IntPtr MemDup<T>(ref T value)
        where T : unmanaged
    {
        void* ptr = NativeMemory.AllocZeroed((nuint)sizeof(T));
        System.Diagnostics.Debug.Assert(ptr != null, "Memory allocation failed");
        NativeMemory.Copy(Unsafe.AsPointer(ref value), ptr, (nuint)sizeof(T));

        return (IntPtr)ptr;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyBack<T>(IntPtr ptr, ref T value)
        where T : unmanaged
    {
        System.Diagnostics.Debug.Assert(ptr != IntPtr.Zero, "Pointer is null");
        NativeMemory.Copy((void*)ptr, Unsafe.AsPointer(ref value), (nuint)sizeof(T));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MemFree(IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            NativeMemory.Free((void*)ptr);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MemFreeReset(ref IntPtr ptr)
    {
        if (ptr != IntPtr.Zero)
        {
            NativeMemory.Free((void*)ptr);
            ptr = IntPtr.Zero;
        }
    }
}
