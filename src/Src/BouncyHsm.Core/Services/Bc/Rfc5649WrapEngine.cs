﻿using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace BouncyHsm.Core.Services.Bc;

/// <summary>
/// An implementation of the AES Key Wrap with Padding specification
/// as described in RFC 5649.
/// See https://tools.ietf.org/html/rfc5649
/// </summary>
public class Rfc5649WrapEngine : IWrapper
{
    private readonly IBlockCipher engine;
    private KeyParameter? param;
    private bool forWrapping;

    private byte[] preIv = { 0xa6, 0xa6, 0xa6, 0xa6 };

    public virtual string AlgorithmName
    {
        get { return this.engine.AlgorithmName + "/RFC5649Wrap"; }
    }

    public Rfc5649WrapEngine(IBlockCipher engine)
    {
        this.engine = engine;
    }

    public void Init(bool forWrapping, ICipherParameters parameters)
    {
        this.forWrapping = forWrapping;

        if (parameters is ParametersWithRandom withRandom)
        {
            parameters = withRandom.Parameters;
        }

        if (parameters is KeyParameter keyParameter)
        {
            this.param = keyParameter;
        }
        else if (parameters is ParametersWithIV withIV)
        {
            this.param = (KeyParameter)withIV.Parameters;
        }
        else
        {
            throw new ArgumentException("parameters is not excepted");
        }
    }

    public byte[] Wrap(byte[] input, int inOff, int length)
    {
        if (!this.forWrapping)
        {
            throw new InvalidOperationException("not set for wrapping");
        }

        byte[] iv = new byte[8];
        Array.Copy(this.preIv, iv, this.preIv.Length);
        BinaryPrimitives.WriteUInt32BigEndian(iv.AsSpan(this.preIv.Length), (uint)length);

        byte[] paddedPlaintext = this.PadPlaintext(input.AsSpan(inOff, length));

        if (paddedPlaintext.Length == 8)
        {
            // if the padded plaintext contains exactly 8 octets,
            // then prepend iv and encrypt using AES in ECB mode.

            // prepend the IV to the plaintext
            byte[] paddedPlainTextWithIV = new byte[paddedPlaintext.Length + iv.Length];
            Array.Copy(iv, 0, paddedPlainTextWithIV, 0, iv.Length);
            Array.Copy(paddedPlaintext, 0, paddedPlainTextWithIV, iv.Length, paddedPlaintext.Length);

            this.engine.Init(true, this.param);
            for (int i = 0; i < paddedPlainTextWithIV.Length; i += this.engine.GetBlockSize())
            {
                this.engine.ProcessBlock(paddedPlainTextWithIV, i, paddedPlainTextWithIV, i);
            }

            return paddedPlainTextWithIV;
        }
        else
        {
            // otherwise, apply the RFC 3394 wrap to
            // the padded plaintext with the new IV
            IWrapper wrapper = new Rfc3394WrapEngine(this.engine);
            ParametersWithIV paramsWithIV = new ParametersWithIV(this.param, iv);
            wrapper.Init(true, paramsWithIV);
            return wrapper.Wrap(paddedPlaintext, 0, paddedPlaintext.Length);
        }
    }

    public byte[] Unwrap(byte[] input, int inOff, int length)
    {
        if (this.forWrapping)
        {
            throw new InvalidOperationException("not set for unwrapping");
        }

        int n = length / 8;

        if ((n * 8) != length)
        {
            throw new InvalidCipherTextException("unwrap data must be a multiple of 8 bytes");
        }

        if (n <= 1)
        {
            throw new InvalidCipherTextException("unwrap data must be at least 16 bytes");
        }

        Span<byte> relevantCiphertext = input.AsSpan(inOff, length);

        byte[] decrypted = new byte[length];
        byte[] paddedPlaintext;
        byte[] extractedAIV;

        if (n == 2)
        {
            // When there are exactly two 64-bit blocks of ciphertext,
            // they are decrypted as a single block using AES in ECB.
            this.engine.Init(false, this.param);
            for (int i = 0; i < relevantCiphertext.Length; i += this.engine.GetBlockSize())
            {
                this.engine.ProcessBlock(relevantCiphertext.Slice(i), decrypted.AsSpan().Slice(i));
            }

            // extract the AIV
            extractedAIV = new byte[8];
            Array.Copy(decrypted, 0, extractedAIV, 0, extractedAIV.Length);
            paddedPlaintext = new byte[decrypted.Length - extractedAIV.Length];
            Array.Copy(decrypted, extractedAIV.Length, paddedPlaintext, 0, paddedPlaintext.Length);
        }
        else
        {
            // Otherwise, unwrap as per RFC 3394 but don't check IV the same way
            decrypted = this.Rfc3394UnwrapNoIvCheck(input, inOff, length, out extractedAIV);
            paddedPlaintext = decrypted;
        }

        // Decompose the extracted AIV to the fixed portion and the MLI
        Span<byte> extractedHighOrderAIV = extractedAIV.AsSpan(0, 4);
        Span<byte> mliBytes = extractedAIV.AsSpan(4, 4);
        int mli = (int)BinaryPrimitives.ReadUInt32BigEndian(mliBytes);
        // Even if a check fails we still continue and check everything 
        // else in order to avoid certain timing based side-channel attacks.
        bool isValid = true;

        // Check the fixed portion of the AIV
        if (!Arrays.FixedTimeEquals(extractedHighOrderAIV, this.preIv))
        {
            isValid = false;
        }

        // Check the MLI against the actual length
        int upperBound = paddedPlaintext.Length;
        int lowerBound = upperBound - 8;
        if (mli <= lowerBound)
        {
            isValid = false;
        }
        if (mli > upperBound)
        {
            isValid = false;
        }

        // Check the number of padding zeros
        int expectedZeros = upperBound - mli;
        if (expectedZeros >= 8 || expectedZeros < 0)
        {
            // We have to pick a "typical" amount of padding to avoid timing attacks.
            isValid = false;
            expectedZeros = 4;
        }

        Span<byte> zeros = stackalloc byte[expectedZeros];
        zeros.Fill(0);
        Span<byte> pad = paddedPlaintext.AsSpan(paddedPlaintext.Length - expectedZeros, expectedZeros);
        if (!Arrays.FixedTimeEquals(pad, zeros))
        {
            isValid = false;
        }

        if (!isValid)
        {
            throw new InvalidCipherTextException("checksum failed");
        }

        // Extract the plaintext from the padded plaintext
        byte[] plaintext = new byte[mli];
        Array.Copy(paddedPlaintext, 0, plaintext, 0, plaintext.Length);

        return plaintext;
    }

    private byte[] PadPlaintext(Span<byte> plaintext)
    {
        int plaintextLength = plaintext.Length;
        int numOfZerosToAppend = (8 - (plaintextLength % 8)) % 8;
        byte[] paddedPlaintext = new byte[plaintextLength + numOfZerosToAppend];
        plaintext.CopyTo(paddedPlaintext);
        if (numOfZerosToAppend != 0)
        {
            // plaintext (i.e., key to be wrapped) does not have
            // a multiple of 8 octet blocks so it must be padded
            paddedPlaintext.AsSpan(plaintextLength, numOfZerosToAppend).Fill(0);
        }

        return paddedPlaintext;
    }

    private byte[] Rfc3394UnwrapNoIvCheck(byte[] input, int inOff, int length, out byte[] extractedAIV)
    {
        const int ivLenght = 8;
        byte[] block = new byte[length - ivLenght];
        byte[] buf = new byte[8 + ivLenght];

        Span<byte> a = input.AsSpan(inOff, ivLenght);
        Array.Copy(input, inOff + ivLenght, block, 0, length - ivLenght);

        this.engine.Init(false, this.param);

        int n = length / 8;
        n = n - 1;

        for (int j = 5; j >= 0; j--)
        {
            for (int i = n; i >= 1; i--)
            {
                a.CopyTo(buf);
                Array.Copy(block, 8 * (i - 1), buf, ivLenght, 8);

                int t = n * j + i;
                for (int k = 1; t != 0; k++)
                {
                    byte v = (byte)t;

                    buf[ivLenght - k] ^= v;

                    t = UnsignedRightShift(t, 8);
                }

                this.engine.ProcessBlock(buf, 0, buf, 0);
                buf.AsSpan(0, 8).CopyTo(a);
                Array.Copy(buf, 8, block, 8 * (i - 1), 8);
            }
        }

        // set the extracted AIV
        extractedAIV = a.ToArray();

        return block;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int UnsignedRightShift(int value, int places)
    {
        unchecked
        {
            uint unsigned = (uint)value;
            unsigned >>= places;
            return (int)unsigned;
        }
    }
}
