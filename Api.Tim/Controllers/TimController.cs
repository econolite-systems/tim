// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Auditing;
using Econolite.Ode.Auditing.Extensions;
using Econolite.Ode.Authorization;
using Econolite.Ode.Services.TimService;
using Microsoft.AspNetCore.Mvc;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Models.Tim.ItisCodes;
using Econolite.Ode.TimService.Models;

namespace Econolite.Ode.Api.Tim.Controllers;

/// <summary>
/// A controller for publishing TIM messages
/// </summary>
[Route("tim")]
[AuthorizeOde(MoundRoadRole.Contributor)]
public class TimController : ControllerBase
{
    private readonly ILogger<TimController> _logger;
    private readonly ITimRequestService _timRequestService;
    private readonly IAuditCrudScopeFactory _auditCrudScopeFactory;
    private readonly string _auditEventType;

    /// <summary>
    /// Constructs a TIM controller
    /// </summary>
    public TimController(
        ITimRequestService timRequestService,
        ILogger<TimController> logger,
        IAuditCrudScopeFactory auditCrudScopeFactory
    )
    {
        _logger = logger;
        _timRequestService = timRequestService;
        _auditCrudScopeFactory = auditCrudScopeFactory;
        _auditEventType = SupportedAuditEventTypes.AuditEventTypes[AuditEventType.TravelerInformationMessages].Event;
    }

    /// <summary>
    /// Get all tim documents
    /// </summary>
    /// <returns>List of tim documents</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TimRsuStatus>))]
    public async Task<ActionResult<IEnumerable<TimRsuStatus>>> GetAsync()
    {
        var results = await _timRequestService.GetAllAsync();
        return Ok(results);
    }
    
    /// <summary>
    /// Get all tim documents
    /// </summary>
    /// <returns>List of tim documents</returns>
    [HttpGet("itis-codes")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ItisCodeType>))]
    public ActionResult<IEnumerable<ItisCodeType>> GetItisCodes()
    {
        var results = ItisCodeExtensions.GetAllItisCodeTypes();
        return Ok(results);
    }
    
    /// <summary>
    /// Get all tim documents
    /// </summary>
    /// <returns>List of tim documents</returns>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TimStatus>))]
    public async Task<ActionResult<IEnumerable<TimStatus>>> GetStatusAsync()
    {
        var results = await _timRequestService.GetStatusAsync();
        return Ok(results);
    }
    
    /// <summary>
    /// Get batch of tim documents by batch id
    /// </summary>
    /// <param name="batchId">Guid for the batch</param>
    /// <returns>List of tim documents for batch</returns>
    [HttpGet("batch/{batchId}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TimDocument>))]
    public async Task<ActionResult<IEnumerable<TimDocument>>> GetBatchAsync(Guid batchId)
    {
        var results = await _timRequestService.GetBatchAsync(batchId);
        return Ok(results);
    }
    
    /// <summary>
    /// Submits a request to publish a text TIM 
    /// </summary>
    [HttpPost("send-request")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TimRequest))]
    public async Task<IActionResult> SendRequestAsync([FromBody] TimRequest tim)
    {
        _logger.LogDebug("Adding {@}", tim);
        var scope = await _auditCrudScopeFactory.CreateAddAsync(_auditEventType, () => tim);
        await using (scope)
        {
            
            return Ok(await _timRequestService.SendRequest(tim));
        }
    }

    /// <summary>
    /// Submits a request to cancel a TIM message
    /// </summary>
    /// <param name="id">The ID of a TIM message</param>
    [HttpPut("cancel/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelAsync(Guid id)
    {
        _logger.LogDebug("Cancelling {@}", id.ToString());
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, id.ToString);
        await using (await scope)
        {
            await _timRequestService.CancelTim(id);

            return Ok();
        }
    }

    /// <summary>
    /// Submits a request to cancel a batch of TIM messages
    /// </summary>
    /// <param name="batchId">The ID of a batch of TIM messages</param>
    [HttpPut("cancel/batch/{batchId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelBatchAsync(Guid batchId)
    {
        _logger.LogDebug("Cancelling {@}", batchId.ToString());
        var scope = _auditCrudScopeFactory.CreateUpdateAsync(_auditEventType, batchId.ToString);
        await using (await scope)
        {
            await _timRequestService.CancelRequest(batchId);

            return Ok();
        }
    }
    
    /// <summary>
    /// Submits a request to delete a TIM message
    /// </summary>
    /// <param name="id">The ID of a TIM message</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        _logger.LogDebug("Deleting {@}", id);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, id.ToString);
        await using (await scope)
        {
            await _timRequestService.DeleteTim(id);

            return Ok();
        }
    }

    /// <summary>
    /// Submits a request to delete a batch of TIM messages
    /// </summary>
    /// <param name="batchId">The ID of a batch of TIM messages</param>
    [HttpDelete("batch/{batchId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteBatchAsync(Guid batchId)
    {
        _logger.LogDebug("Deleting {@}", batchId);
        var scope = _auditCrudScopeFactory.CreateDeleteAsync(_auditEventType, batchId.ToString);
        await using (await scope)
        {
            await _timRequestService.DeleteBatch(batchId);

            return Ok();
        }
    }
}
