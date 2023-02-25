namespace BouncyHsm.Core.Rpc;

public static class HeadEncoder
{
    public const int Size = 8;

    public static (int HeaderSize, int BodySize) Decode(Span<byte> input)
    {
        System.Diagnostics.Debug.Assert(input.Length == Size);

        if (input[0] != 0xBC)
        {
            throw new ArgumentException("Header must start 0xBC.", nameof(input));
        }

        if (input[1] != 0)
        {
            throw new ArgumentException("Protocol version must by 0.", nameof(input));
        }


        int headerSize = input[3];
        headerSize |= ((int)input[2]) << 8;

        int bodySize = input[7];
        bodySize |= ((int)input[6]) << 8;
        bodySize |= ((int)input[5]) << 16;
        bodySize |= ((int)input[4]) << 24;

        return (headerSize, bodySize);
    }

    public static byte[] Encode(int headerSize, int bodySize)
    {
        byte[] responseHeader = new byte[Size];
        Encode(headerSize, bodySize, responseHeader);

        return responseHeader;
    }

    public static void Encode(int headerSize, int bodySize, Span<byte> responseHeader)
    {
        System.Diagnostics.Debug.Assert(responseHeader.Length == Size);

        responseHeader[0] = 0xBC;
        responseHeader[1] = 0;

        responseHeader[2] = (byte)((headerSize >> 8) & 0xFF);
        responseHeader[3] = (byte)(headerSize & 0xFF);

        responseHeader[4] = (byte)((bodySize >> 24) & 0xFF);
        responseHeader[5] = (byte)((bodySize >> 16) & 0xFF);
        responseHeader[6] = (byte)((bodySize >> 8) & 0xFF);
        responseHeader[7] = (byte)(bodySize & 0xFF);
    }

    public static void Encode(ReadOnlyMemory<byte> header, ReadOnlyMemory<byte> body, Memory<byte> result)
    {
        Encode(header.Length, body.Length, result.Span);
    }
}