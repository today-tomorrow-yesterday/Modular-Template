using FluentValidation.TestHelper;
using Modules.Sales.Application.Packages.UpdatePackageSalesTeam;
using Modules.Sales.Domain.Packages.SalesTeam;
using Xunit;

namespace Modules.Sales.Application.Tests.SalesTeam;

public sealed class UpdatePackageSalesTeamCommandValidatorTests
{
    private readonly UpdatePackageSalesTeamCommandValidator _sut = new();

    [Fact]
    public void Valid_command_with_single_primary_passes()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [new(1, SalesTeamRole.Primary, 100m)]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_command_with_two_members_passes()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [
                new(1, SalesTeamRole.Primary, 60m),
                new(2, SalesTeamRole.Secondary, 40m)
            ]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Valid_command_without_split_percentages_passes()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [
                new(1, SalesTeamRole.Primary, null),
                new(2, SalesTeamRole.Secondary, null)
            ]);

        var result = _sut.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_package_public_id_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.Empty,
            [new(1, SalesTeamRole.Primary, 100m)]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PackagePublicId);
    }

    [Fact]
    public void Empty_members_array_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(Guid.NewGuid(), []);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Members);
    }

    [Fact]
    public void More_than_two_members_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [
                new(1, SalesTeamRole.Primary, 50m),
                new(2, SalesTeamRole.Secondary, 30m),
                new(3, (SalesTeamRole)99, 20m)
            ]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Members);
    }

    [Fact]
    public void Missing_primary_role_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [new(1, SalesTeamRole.Secondary, 100m)]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Members);
    }

    [Fact]
    public void Duplicate_roles_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [
                new(1, SalesTeamRole.Primary, 50m),
                new(2, SalesTeamRole.Primary, 50m)
            ]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Members);
    }

    [Fact]
    public void Split_percentages_not_summing_to_100_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [
                new(1, SalesTeamRole.Primary, 60m),
                new(2, SalesTeamRole.Secondary, 30m)
            ]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Members);
    }

    [Fact]
    public void Negative_split_percentage_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [new(1, SalesTeamRole.Primary, -10m)]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Split_percentage_over_100_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [new(1, SalesTeamRole.Primary, 150m)]);

        var result = _sut.TestValidate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Mixed_split_percentages_some_with_some_without_fails()
    {
        var command = new UpdatePackageSalesTeamCommand(
            Guid.NewGuid(),
            [
                new(1, SalesTeamRole.Primary, 100m),
                new(2, SalesTeamRole.Secondary, null)
            ]);

        var result = _sut.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Members);
    }
}
