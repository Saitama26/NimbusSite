using AutoMapper;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Users.Application.Abstractions;
using Users.Application.DTOs;
using Users.Domain.Errors;

namespace Users.Application.Queries.GetUserById;

/// <summary>
/// Запрос получения пользователя по ID
/// </summary>
public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserDto>;

/// <summary>
/// Обработчик запроса получения пользователя по ID
/// </summary>
internal sealed class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<UserDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        // Найти пользователя
        var user = await _repository.GetByIdAsync(query.UserId, cancellationToken);
        if (user == null)
        {
            return Result<UserDto>.Failure(UserErrors.NotFound(query.UserId));
        }

        // Маппинг в DTO
        var dto = _mapper.Map<UserDto>(user);

        return Result<UserDto>.Success(dto);
    }
}

