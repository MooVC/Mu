namespace Mu.Auditing.Configuration;

using System;
using System.Collections.Generic;
using System.Text;

public sealed record AuditOptions(AuditOperationScope Mutational = AuditOperationScope.All, AuditOperationScope NonMutational = AuditOperationScope.External);