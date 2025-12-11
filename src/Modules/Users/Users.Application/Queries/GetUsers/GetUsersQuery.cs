using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Users.Application.Abstractions;
using Users.Application.DTOs;

namespace Users.Application.Queries.GetUsers;

/// <summary>
/// Запрос получения списка пользователей
/// OData обрабатывает пагинацию, фильтрацию и сортировку через запросы ($skip, $top, $filter, $orderby)
/// </summary>
public sealed record GetUsersQuery() : IQuery<IQueryable<UserListItemDto>>;

/// <summary>
/// Обработчик запроса получения списка пользователей
/// Возвращает IQueryable для OData пагинации, фильтрации и сортировки
/// </summary>
internal sealed class GetUsersQueryHandler : IQueryHandler<GetUsersQuery, IQueryable<UserListItemDto>>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<IQueryable<UserListItemDto>>> Handle(GetUsersQuery query, CancellationToken cancellationToken)
    {
        // Получить IQueryable пользователей
        var usersQueryable = await _repository.GetAllAsync(cancellationToken);

        // Маппинг через AutoMapper в DTO (ProjectTo для IQueryable)
        var dtoQueryable = usersQueryable.ProjectTo<UserListItemDto>(_mapper.ConfigurationProvider);

        return Result<IQueryable<UserListItemDto>>.Success(dtoQueryable);
    }
}

