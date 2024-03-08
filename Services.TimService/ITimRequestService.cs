// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.TimService.Models;

namespace Econolite.Ode.Services.TimService;

public interface ITimRequestService
{
    Task<IEnumerable<TimRsuStatus>> GetAllAsync();
    Task<IEnumerable<TimStatus>> GetStatusAsync();
    Task<IEnumerable<TimRsuStatus>> GetBatchAsync(Guid timId);
    Task<TimRequest> SendRequest(TimRequest request, TimSource source = TimSource.ManualEntry);
    Task SendUpdate(IEnumerable<TimRsuStatus> requests, CancellationToken stoppingToken = default);
    Task CancelRequest(Guid batchId);
    Task CancelTim(Guid timId);
    Task DeleteTim(Guid timId);
    Task DeleteBatch(Guid batchId);
}
