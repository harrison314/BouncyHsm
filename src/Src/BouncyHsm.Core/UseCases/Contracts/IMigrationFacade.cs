using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.UseCases.Contracts;

public interface IMigrationFacade
{
    ValueTask<DomainResult<MigrationResult>> Migrate(CancellationToken cancellationToken);
}
