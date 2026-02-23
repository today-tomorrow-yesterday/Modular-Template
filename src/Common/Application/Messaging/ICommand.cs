using MediatR;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Application.Messaging;

/// <summary>
/// Marker interface for commands.
/// </summary>
public interface IBaseCommand;

/// <summary>
/// Represents a command that returns no value.
/// </summary>
public interface ICommand : IRequest<Result>, IBaseCommand;

/// <summary>
/// Represents a command that returns a value.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;
