using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Tenants.Application.Commands.ChangeTenantStatus;
using Tenants.Application.Commands.CreateTenant;
using Tenants.Application.Commands.DeleteTenant;
using Tenants.Application.Commands.UpdateTenant;
using Tenants.Application.Queries.GetTenantById;
using Tenants.Application.Queries.GetTenants;
using Tenants.Contracts.Api.Requests;
using Tenants.Contracts.Api.Responses;
using Tenants.Contracts.Enums;
using Tenants.Domain.Enums;

namespace Tenants.Api.Controllers;

/// <summary>
/// Контроллер для управления тенантами
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TenantsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TenantsController> _logger;

    public TenantsController(
        ISender sender,
        IConfiguration configuration,
        ILogger<TenantsController> logger)
    {
        _sender = sender;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Получить список всех тенантов
    /// </summary>
    /// <returns>Список тенантов</returns>
    /// <response code="200">Успешно получен список тенантов</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TenantListResponse>>> GetTenants(CancellationToken cancellationToken)
    {
        var query = new GetTenantsQuery();
        var result = await _sender.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        // Маппинг внутренних DTOs в Contracts API Responses
        var responses = result.Value!.Select(dto => new TenantListResponse(
            dto.TenantInt,
            dto.Name,
            (TenantStatusContract)(int)dto.Status,
            dto.CreatedAt));

        return Ok(responses.ToList());
    }

    /// <summary>
    /// Получить тенанта по числовому идентификатору
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <returns>Информация о тенанте</returns>
    /// <response code="200">Тенант найден</response>
    /// <response code="404">Тенант не найден</response>
    [HttpGet("{tenantInt:int}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantResponse>> GetTenantById(int tenantInt, CancellationToken cancellationToken)
    {
        var query = new GetTenantByIdQuery(tenantInt);
        var result = await _sender.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Type == ErrorType.NotFound)
            {
                return NotFound(result.Error.Description);
            }
            return Problem(result.Error?.Description, statusCode: MapStatus(result.Error));
        }

        // Маппинг внутреннего DTO в Contracts API Response
        var dto = result.Value!;
        var response = new TenantResponse(
            dto.TenantInt,
            dto.Name,
            (TenantStatusContract)(int)dto.Status,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.Description,
            dto.ConnectionString);

        return Ok(response);
    }

    /// <summary>
    /// Создать нового тенанта
    /// </summary>
    /// <param name="request">Данные для создания тенанта</param>
    /// <returns>Созданный тенант</returns>
    /// <response code="201">Тенант успешно создан</response>
    /// <response code="400">Ошибка валидации данных</response>
    [HttpPost]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TenantResponse>> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        // Маппинг API Request в Internal Command
        // ConnectionString генерируется автоматически на основе TenantInt после создания
        var command = new CreateTenantCommand(
            request.Name,
            request.Description);

        var result = await _sender.Send<CreateTenantCommand, CreateTenantResponse>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Description ?? "Failed to create tenant" });
        }

        // Получаем созданный тенант для полного ответа (Name, Status, Description и т.д.)
        var getQuery = new GetTenantByIdQuery(result.Value!.TenantInt);
        var getResult = await _sender.Send(getQuery, cancellationToken);

        if (!getResult.IsSuccess)
        {
            return Problem(getResult.Error?.Description, statusCode: MapStatus(getResult.Error));
        }

        var dto = getResult.Value!;
        var response = new TenantResponse(
            dto.TenantInt,
            dto.Name,
            (TenantStatusContract)(int)dto.Status,
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.Description,
            dto.ConnectionString);

        return CreatedAtAction(
            nameof(GetTenantById),
            new { tenantInt = response.TenantInt },
            response);
    }

    /// <summary>
    /// Обновить информацию о тенанте
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <param name="request">Данные для обновления</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Тенант успешно обновлен</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="404">Тенант не найден</response>
    [HttpPut("{tenantInt:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenant(
        int tenantInt,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        // Маппинг API Request в Internal Command
        var command = new UpdateTenantCommand(
            tenantInt,
            request.Name,
            request.Description);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Type == ErrorType.NotFound)
            {
                return NotFound(new { error = result.Error.Description ?? "Tenant not found" });
            }
            return BadRequest(new { error = result.Error?.Description ?? "Failed to update tenant" });
        }

        return NoContent();
    }

    /// <summary>
    /// Удалить тенанта (soft delete)
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Тенант успешно удален</response>
    /// <response code="404">Тенант не найден</response>
    [HttpDelete("{tenantInt:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenant(int tenantInt, CancellationToken cancellationToken)
    {
        var command = new DeleteTenantCommand(tenantInt);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error?.Description ?? "Tenant not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Изменить статус тенанта
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <param name="request">Новый статус</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Статус успешно изменен</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="404">Тенант не найден</response>
    [HttpPatch("{tenantInt:int}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeTenantStatus(
        int tenantInt,
        [FromBody] ChangeTenantStatusRequest request,
        CancellationToken cancellationToken)
    {
        // Маппинг API Request в Internal Command
        var newStatus = (TenantStatus)(int)request.NewStatus;
        var command = new ChangeTenantStatusCommand(tenantInt, newStatus);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Type == ErrorType.NotFound)
            {
                return NotFound(new { error = result.Error.Description ?? "Tenant not found" });
            }
            return BadRequest(new { error = result.Error?.Description ?? "Failed to change tenant status" });
        }

        return NoContent();
    }

    /// <summary>
    /// Обновить строку подключения тенанта
    /// </summary>
    /// <param name="tenantInt">Числовой идентификатор тенанта</param>
    /// <param name="request">Новая строка подключения</param>
    /// <returns>Результат операции с TenantInt и обновленной connection string</returns>
    /// <response code="200">Строка подключения успешно обновлена</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="404">Тенант не найден</response>
    [HttpPut("{tenantInt:int}/connection-string")]
    [ProducesResponseType(typeof(Tenants.Contracts.Api.Responses.UpdateTenantConnectionStringResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Tenants.Contracts.Api.Responses.UpdateTenantConnectionStringResponse>> UpdateTenantConnectionString(
        int tenantInt,
        [FromBody] UpdateTenantConnectionStringRequest request,
        CancellationToken cancellationToken)
    {
        var command = new Tenants.Application.Commands.UpdateTenantConnectionString.UpdateTenantConnectionStringCommand(tenantInt, request.ConnectionString);
        var result = await _sender.Send<Tenants.Application.Commands.UpdateTenantConnectionString.UpdateTenantConnectionStringCommand, Tenants.Application.Commands.UpdateTenantConnectionString.UpdateTenantConnectionStringResponse>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Type == ErrorType.NotFound)
            {
                return NotFound(new { error = result.Error.Description ?? "Tenant not found" });
            }
            return BadRequest(new { error = result.Error?.Description ?? "Failed to update connection string" });
        }

        // Маппинг внутреннего Response в Contracts API Response
        var response = result.Value!;
        var apiResponse = new Tenants.Contracts.Api.Responses.UpdateTenantConnectionStringResponse(
            response.TenantInt,
            response.ConnectionString);

        return Ok(apiResponse);
    }

    private static int? MapStatus(Error? error)
    {
        if (error == null) return 500;
        return error.Type switch
        {
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => 500
        };
    }
}

/// <summary>
/// Запрос на изменение статуса тенанта (временный, будет заменен на Contracts)
/// </summary>
public record ChangeTenantStatusRequest(
    TenantStatusContract NewStatus);
