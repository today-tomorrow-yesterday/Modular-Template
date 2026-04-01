using MediatR;
using ModularTemplate.Domain.Results;

namespace ModularTemplate.Application.Messaging;

/// <summary>
/// Handler for queries.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
