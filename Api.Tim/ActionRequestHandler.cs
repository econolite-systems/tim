// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Router.ActionSet.Messaging;
using Econolite.Ode.Services.TimService;
using Econolite.Ode.TimService.Models;

namespace Econolite.Ode.Api.Tim;

public class ActionRequestHandler : BackgroundService
{
    private readonly ISource<ActionRequest> _source;
    private readonly ITimRequestService _service;
    private readonly ILogger<ActionRequestHandler> _logger;
    private readonly IMetricsCounter _messageCounter;

    public ActionRequestHandler(IServiceProvider provider, ISource<ActionRequest> source, IMetricsFactory metricsFactory, ILogger<ActionRequestHandler> logger)
    {
        _source = source;
        _service = provider.CreateScope().ServiceProvider.GetRequiredService<ITimRequestService>();
        _logger = logger;
        _messageCounter = metricsFactory.GetMetricsCounter("Action Request");
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting to consume Action Request");
        try
        {
            await _source.ConsumeOnAsync(async result =>
            {
                _messageCounter.Increment();
                if (result.Value.ActionType == "send-tim-message")
                {
                    await HandleSendTimAsync(result, stoppingToken);
                }
            }, stoppingToken);
        }
        finally
        {
            _logger.LogInformation("Ending Action Request consumption");
        }
    }

    private async Task HandleSendTimAsync(ConsumeResult<Guid, ActionRequest> result, CancellationToken stoppingToken)
    {
        var timRequest = result.Value.ToTimRequest();
        await _service.SendRequest(timRequest, TimSource.LogicStatement);
    }
}

/// <summary>
/// Extension methods for <see cref="ActionRequestHandler"/>
/// </summary>
public static class ActionRequestHandlerExtensions
{
    /// <summary>
    /// Convert <see cref="ActionRequest"/> to <see cref="TimRequest"/>
    /// </summary>
    /// <param name="action"></param>
    /// <returns></returns>
    public static TimRequest ToTimRequest(this ActionRequest action)
    {
        var targetType = action.TargetType.ToTargetType();
        return new TimRequest()
        {
            Id = action.Id,
            Cancel = action.Cancel,
            TransmitMode = action.TransmitMode.ToTimTransmitMode(),
            DurationType = action.DurationType.ToDurationType(),
            Duration = int.Parse(action.Duration ?? "1"),
            Longitude = action.Longitude,
            Latitude = action.Latitude,
            TargetType = targetType,
            Parameters = action.Parameter,
            Target = targetType == TargetType.Target ? action.Parameter.Select(Guid.Parse) : Array.Empty<Guid>(),
            ItisCode = action.Info.ToItisCode(),
            MessageType = action.MessageType.ToMessageType()
        };
    }
}
