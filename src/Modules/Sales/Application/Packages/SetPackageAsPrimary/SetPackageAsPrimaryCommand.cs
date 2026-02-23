using Rtl.Core.Application.Messaging;

namespace Modules.Sales.Application.Packages.SetPackageAsPrimary;

public sealed record SetPackageAsPrimaryCommand(Guid PackagePublicId) : ICommand;
