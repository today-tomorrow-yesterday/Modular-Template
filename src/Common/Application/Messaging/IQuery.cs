using MediatR;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Application.Messaging;

/// <summary>
/// Represents a query that returns a value.
/// </summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
