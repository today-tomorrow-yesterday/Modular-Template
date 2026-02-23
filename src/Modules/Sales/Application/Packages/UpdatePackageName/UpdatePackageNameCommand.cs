using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.UpdatePackageName;

public sealed record UpdatePackageNameCommand(Guid PackagePublicId, string Name) : ICommand;
