using BouncyHsm.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;


namespace Aspire.Hosting;

public delegate Task BouncyHsmInitializeTask(BouncyHsmClient client, ILogger logger, CancellationToken cancellationToken);
