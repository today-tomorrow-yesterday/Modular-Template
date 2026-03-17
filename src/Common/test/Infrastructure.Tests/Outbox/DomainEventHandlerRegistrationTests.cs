using Microsoft.Extensions.DependencyInjection;
using Rtl.Core.Application.Messaging;
using Rtl.Core.Infrastructure.EventBus;
using Rtl.Core.Domain.Events;
using Rtl.Core.Infrastructure.Outbox.Handler;
using System.Reflection;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.Outbox;

public class DomainEventHandlerRegistrationTests
{
    // ─── Test fixtures ───────────────────────────────────────────────

    public sealed record TestDomainEvent() : DomainEvent;

    public sealed class TestDependency
    {
        public bool WasCalled { get; set; }
    }

    public sealed class TestDomainEventHandler(TestDependency dep) : DomainEventHandler<TestDomainEvent>
    {
        public override Task Handle(TestDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            dep.WasCalled = true;
            return Task.CompletedTask;
        }
    }

    // ─── Tests ───────────────────────────────────────────────────────

    [Fact]
    public void Factory_discovers_handler_types_from_assembly()
    {
        // The factory should find TestDomainEventHandler in this test assembly
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();
        services.AddScoped<TestDomainEventHandler>();
        using var sp = services.BuildServiceProvider();

        var handlers = DomainEventHandlersFactory.GetHandlers(
            typeof(TestDomainEvent),
            sp,
            Assembly.GetExecutingAssembly());

        Assert.Single(handlers);
    }

    [Fact]
    public void Factory_throws_when_handler_is_not_registered_in_di()
    {
        // Proves the bug: factory finds the handler type but DI cannot resolve it.
        var services = new ServiceCollection();
        // NOTE: TestDomainEventHandler is NOT registered
        using var sp = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() =>
            DomainEventHandlersFactory.GetHandlers(
                typeof(TestDomainEvent),
                sp,
                Assembly.GetExecutingAssembly()).ToList());
    }

    [Fact]
    public async Task Handler_executes_when_properly_registered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();

        // Register handler the way AddDomainEventHandlers should
        services.AddScoped<TestDomainEventHandler>();

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var handlers = DomainEventHandlersFactory.GetHandlers(
            typeof(TestDomainEvent),
            scope.ServiceProvider,
            Assembly.GetExecutingAssembly());

        var domainEvent = new TestDomainEvent();

        foreach (var handler in handlers)
        {
            await handler.Handle(domainEvent);
        }

        var dep = scope.ServiceProvider.GetRequiredService<TestDependency>();
        Assert.True(dep.WasCalled);
    }

    [Fact]
    public void AddDomainEventHandlers_registers_all_handlers_from_assembly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();

        // This is the method we're adding to fix the registration gap
        services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());

        using var sp = services.BuildServiceProvider();

        // Should be resolvable now without explicit per-handler registration
        var handler = sp.GetService<TestDomainEventHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public async Task Full_pipeline_works_with_AddDomainEventHandlers()
    {
        // End-to-end: register via convention → factory discovers → DI resolves → handler executes
        var services = new ServiceCollection();
        services.AddSingleton<TestDependency>();
        services.AddDomainEventHandlers(Assembly.GetExecutingAssembly());

        using var sp = services.BuildServiceProvider();
        using var scope = sp.CreateScope();

        var handlers = DomainEventHandlersFactory.GetHandlers(
            typeof(TestDomainEvent),
            scope.ServiceProvider,
            Assembly.GetExecutingAssembly());

        Assert.Single(handlers);

        var domainEvent = new TestDomainEvent();
        foreach (var handler in handlers)
        {
            await handler.Handle(domainEvent);
        }

        var dep = scope.ServiceProvider.GetRequiredService<TestDependency>();
        Assert.True(dep.WasCalled, "Handler should have been invoked via the full factory → DI → execute pipeline");
    }
}
