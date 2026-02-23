using MediatR;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Application.Messaging;

/// <summary>
/// Handler for commands that return no value.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>
/// Handler for commands that return a value.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;
