using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.Common;

public static class CKT_V3_2
{
    /// <summary>
    /// Default value - the certificate is neither trusted nor untrusted for the associated operation 
    /// </summary>
    public const ulong CKT_TRUST_UNKNOWN = 0x00000000U;

    /// <summary>
    /// the certificate is trusted for the associated operation
    /// </summary>
    public const ulong CKT_TRUSTED = 0x00000001U;

    /// <summary>
    /// the certificate is trusted as a root signing certificate for chain validation of a cert that is trusted for the associate operation; this applies even when the certificate is not self-signed and when the certificate does not have the proper attributes to be CA certificate
    /// </summary>
    public const ulong CKT_TRUST_ANCHOR = 0x00000002U;

    /// <summary>
    /// the certificate is explicitly not trusted for the associated operation, nor can trust chain through the certificate to an otherwise trusted root; this attribute can be used to ‘revoke’ intermediate CA certificates that have been compromised without removing trust from the parent certificate
    /// </summary>
    public const ulong CKT_NOT_TRUSTED = 0x00000003U;

    /// <summary>
    /// the certificate is neither trusted nor untrusted for the associated operation 
    /// </summary>
    public const ulong CKT_TRUST_MUST_VERIFY_TRUST = 0x00000004U;
}
