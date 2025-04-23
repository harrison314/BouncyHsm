using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

[Dunet.Union]
internal partial record CipherUinion
{
    partial record BufferedCipher(IBufferedCipher Buffered);
    partial record StreamCipher(IStreamCipher Stream);
    partial record AeadCipher(IAeadCipher Aead);
}