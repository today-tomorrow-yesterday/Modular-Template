using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.DeletePackage;

public sealed record DeletePackageCommand(Guid PackagePublicId) : ICommand;
