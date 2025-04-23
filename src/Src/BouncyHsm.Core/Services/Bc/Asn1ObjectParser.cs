using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Bc;

internal static class Asn1ObjectParser
{
    public static Asn1Object FromByteArray(byte[] data, bool accetExtraData)
    {
        System.Diagnostics.Debug.Assert(data != null);

        try
        {
            int limit = data.Length;
            using MemoryStream ms = new MemoryStream(data, false);
            using Asn1InputStream asn1In = new Asn1InputStream(ms,
                limit,
                leaveOpen: true);

            Asn1Object result = asn1In.ReadObject();

            if (!accetExtraData && asn1In.Position != limit)
            {
                throw new IOException("extra data found after object");
            }

            return result;

        }
        catch (InvalidCastException)
        {
            throw new IOException("cannot recognise object in byte array");
        }
    }
}
