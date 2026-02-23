using MediatR;
using Rtl.Core.Domain.Results;

namespace Rtl.Core.Application.Messaging;

/// <summary>
/// Handler for queries.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
