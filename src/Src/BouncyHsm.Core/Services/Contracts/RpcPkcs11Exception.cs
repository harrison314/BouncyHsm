using BouncyHsm.Core.Services.Contracts.P11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts;

[Serializable]
public class RpcPkcs11Exception : ApplicationException
{
    public CKR ReturnValue
    {
        get;
        protected set;
    }

    public RpcPkcs11Exception(CKR returnValue, string message)
        : base(message)
    {
        this.ReturnValue = returnValue;
    }

    public RpcPkcs11Exception(CKR returnValue, string message, Exception? innerException)
        : base(message, innerException)
    {
        this.ReturnValue = returnValue;
    }
}
