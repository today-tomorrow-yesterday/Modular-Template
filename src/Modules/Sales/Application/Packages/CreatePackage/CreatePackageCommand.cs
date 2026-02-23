using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.CreatePackage;

public sealed record CreatePackageCommand(
    Guid SalePublicId,
    string Name) : ICommand<CreatePackageResult>;

public sealed record CreatePackageResult(Guid PublicId);
