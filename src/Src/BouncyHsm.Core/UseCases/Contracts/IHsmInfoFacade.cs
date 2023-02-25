using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Contracts;

public interface IHsmInfoFacade
{
    IEnumerable<SupportedNameCurve> GetCurves();

    BouncyHsmVersion GetVersions();

    IEnumerable<MechanismInfoData> GetAllMechanism();
}
