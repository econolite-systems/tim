// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Commands.Rsu.Messaging;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Repository.TimService;
using Action = Econolite.Ode.Commands.Rsu.Messaging.Action;

namespace Econolite.Ode.Api.Tim;

public class TimCommandResponseHandler : BackgroundService
{
    private readonly ISource<RsuCommandResponse> _rsuCommandResponseSource;
    private readonly ITimRsuStatusRepository _timRepository;
    private readonly ILogger<TimCommandResponseHandler> _logger;
    private readonly IMetricsCounter _messageCounter;

    public TimCommandResponseHandler(ISource<RsuCommandResponse> rsuCommandResponseSource, ITimRsuStatusRepository timRepository, IMetricsFactory metricsFactory, ILogger<TimCommandResponseHandler> logger)
    {
        _rsuCommandResponseSource = rsuCommandResponseSource;
        _timRepository = timRepository;
        _logger = logger;
        _messageCounter = metricsFactory.GetMetricsCounter("Rsu command response");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting to consume RsuCommandRequest");
        try
        {
            await _rsuCommandResponseSource.ConsumeOnAsync(async result =>
            {
                _messageCounter.Increment();
                if (result.Type == "TimCommandResponse")
                {
                    await HandleTimCommandResponseAsync(result, stoppingToken);
                }
            }, stoppingToken);
        }
        finally
        {
            _logger.LogInformation("Ending RsuCommandRequest consumption");
        }
    }

    private async Task HandleTimCommandResponseAsync(ConsumeResult<Guid, RsuCommandResponse> result, CancellationToken stoppingToken)
    {
        try
        {
            var timCommandResponse = result.ToObject<TimCommandResponse>();
            if (timCommandResponse is null)
            {
                return;
            }
            var tim = await _timRepository.GetByIdAsync(timCommandResponse.Id);
            if (tim is null)
            {
                throw new NullReferenceException($"Tim with id {timCommandResponse.Id} not found");
            }
            tim.Index = timCommandResponse.Index;
            tim.Error = timCommandResponse.Error;
            tim.State = timCommandResponse.IsSuccess ? tim.State == TimState.Canceling ? 
                tim.CancelOnDuration? TimState.Stopped : TimState.Canceling : 
                tim.Action == Action.Delete ? TimState.Stopped :
                TimState.Running : 
                string.IsNullOrEmpty(timCommandResponse.Error) ? TimState.Stopped : TimState.Error;
            tim.Enable = timCommandResponse.IsSuccess;
            if (tim.State == TimState.Stopped)
            {
                tim.EndDate = DateTime.UtcNow;
                tim.Deleted = true;
            }
            _timRepository.Update(tim);
            await _timRepository.DbContext.SaveChangesAsync(stoppingToken);
        } catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling TimCommandResponse");
        }
    }
}
