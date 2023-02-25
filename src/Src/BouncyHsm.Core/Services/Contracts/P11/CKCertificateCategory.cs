using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.P11;

public enum CKCertificateCategory : uint
{
    CK_CERTIFICATE_CATEGORY_UNSPECIFIED = 0,
    CK_CERTIFICATE_CATEGORY_TOKEN_USER = 1,
    CK_CERTIFICATE_CATEGORY_AUTHORITY = 2,
    CK_CERTIFICATE_CATEGORY_OTHER_ENTITY = 3
}
