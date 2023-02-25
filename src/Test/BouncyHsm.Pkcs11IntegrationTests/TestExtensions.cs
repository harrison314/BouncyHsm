using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Pkcs11IntegrationTests;

internal static class TestExtensions
{
    public static ISlot SelectTestSlot(this List<ISlot> slots)
    {
        //TODO: update selection of test slot
        return slots.First();
    }
}
