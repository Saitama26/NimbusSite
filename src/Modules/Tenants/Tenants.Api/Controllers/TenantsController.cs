using Common.Application.Abstractions.Messaging;
using Common.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using Tenants.Application.Commands.ChangeTenantStatus;
using Tenants.Application.Commands.CreateTenant;
using Tenants.Application.Commands.DeleteTenant;
using Tenants.Application.Commands.UpdateTenant;
using Tenants.Application.Commands.UpdateTenantConnectionString;
using Tenants.Application.DTOs;
using Tenants.Application.Queries.GetTenantById;
using Tenants.Application.Queries.GetTenants;
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

    public TenantsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Получить список всех тенантов
    /// </summary>
    /// <returns>Список тенантов</returns>
    /// <response code="200">Успешно получен список тенантов</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TenantListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TenantListItemDto>>> GetTenants(CancellationToken cancellationToken)
    {
        var query = new GetTenantsQuery();
        var result = await _sender.Send<IQueryable<TenantListItemDto>>(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Description ?? "Failed to get tenants" });
        }

        return Ok(result.Value?.ToList() ?? new List<TenantListItemDto>());
    }

    /// <summary>
    /// Получить тенанта по ID
    /// </summary>
    /// <param name="tenantId">Идентификатор тенанта</param>
    /// <returns>Информация о тенанте</returns>
    /// <response code="200">Тенант найден</response>
    /// <response code="404">Тенант не найден</response>
    [HttpGet("{tenantId:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> GetTenantById(Guid tenantId, CancellationToken cancellationToken)
    {
        var query = new GetTenantByIdQuery(tenantId);
        var result = await _sender.Send<TenantDto>(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { error = result.Error?.Description ?? "Tenant not found" });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Создать нового тенанта
    /// </summary>
    /// <param name="command">Данные для создания тенанта</param>
    /// <returns>Созданный тенант</returns>
    /// <response code="201">Тенант успешно создан</response>
    /// <response code="400">Ошибка валидации данных</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTenantResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateTenantResponse>> CreateTenant(
        [FromBody] CreateTenantCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send<CreateTenantCommand, CreateTenantResponse>(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error?.Description ?? "Failed to create tenant" });
        }

        return CreatedAtAction(
            nameof(GetTenantById),
            new { tenantId = result.Value!.TenantId },
            result.Value);
    }

    /// <summary>
    /// Обновить информацию о тенанте
    /// </summary>
    /// <param name="tenantId">Идентификатор тенанта</param>
    /// <param name="request">Данные для обновления</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Тенант успешно обновлен</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="404">Тенант не найден</response>
    [HttpPut("{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenant(
        Guid tenantId,
        [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTenantCommand(
            tenantId,
            request.Name,
            request.Description,
            request.AdminEmail);

        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "NotFound")
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
    /// <param name="tenantId">Идентификатор тенанта</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Тенант успешно удален</response>
    /// <response code="404">Тенант не найден</response>
    [HttpDelete("{tenantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        var command = new DeleteTenantCommand(tenantId);
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
    /// <param name="tenantId">Идентификатор тенанта</param>
    /// <param name="request">Новый статус</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Статус успешно изменен</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="404">Тенант не найден</response>
    [HttpPatch("{tenantId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeTenantStatus(
        Guid tenantId,
        [FromBody] ChangeTenantStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangeTenantStatusCommand(tenantId, request.NewStatus);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "NotFound")
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
    /// <param name="tenantId">Идентификатор тенанта</param>
    /// <param name="request">Новая строка подключения</param>
    /// <returns>Результат операции</returns>
    /// <response code="204">Строка подключения успешно обновлена</response>
    /// <response code="400">Ошибка валидации данных</response>
    /// <response code="404">Тенант не найден</response>
    [HttpPut("{tenantId:guid}/connection-string")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenantConnectionString(
        Guid tenantId,
        [FromBody] UpdateTenantConnectionStringRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTenantConnectionStringCommand(tenantId, request.ConnectionString);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.Error?.Code == "NotFound")
            {
                return NotFound(new { error = result.Error.Description ?? "Tenant not found" });
            }
            return BadRequest(new { error = result.Error?.Description ?? "Failed to update connection string" });
        }

        return NoContent();
    }
}

/// <summary>
/// Запрос на обновление тенанта
/// </summary>
public record UpdateTenantRequest(
    string? Name = null,
    string? Description = null,
    string? AdminEmail = null);

/// <summary>
/// Запрос на изменение статуса тенанта
/// </summary>
public record ChangeTenantStatusRequest(
    /// <summary>
    /// Новый статус тенанта (1 = Active, 2 = Suspended, 3 = Deleted)
    /// </summary>
    TenantStatus NewStatus);

/// <summary>
/// Запрос на обновление строки подключения тенанта
/// </summary>
public record UpdateTenantConnectionStringRequest(
    /// <summary>
    /// Строка подключения к базе данных
    /// </summary>
    string ConnectionString);

