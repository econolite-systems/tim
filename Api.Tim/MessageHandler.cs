// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Common.Scheduler.Base.Timers;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Services.TimService;
using Econolite.Ode.TimService.Models;
using Action = Econolite.Ode.Commands.Rsu.Messaging.Action;

namespace Econolite.Ode.Api.Tim;

public class MessageHandler : BackgroundService
{
    private readonly ITimRequestService _timRequestService;
    private readonly IPeriodicTimer _timer;

    public MessageHandler(IServiceProvider provider, IPeriodicTimerFactory timerFactory)
    {
        _timRequestService = provider.CreateScope().ServiceProvider.GetRequiredService<ITimRequestService>();
        _timer = timerFactory.CreateTopOfMinuteTimer();
    }
    
    private async Task RunAsync()
    {
        var messages = await _timRequestService.GetAllAsync();
        var messagesToRun = GetTimRequestToRun(messages);
        await _timRequestService.SendUpdate(messagesToRun);
    }

    private IEnumerable<TimRsuStatus> GetTimRequestToRun(IEnumerable<TimRsuStatus> messages)
    {
        var result = new List<TimRsuStatus>();
        foreach (var message in messages.Where(
                     m => 
                         !m.Deleted ||
                         m.State != TimState.Pending ||
                         m.State != TimState.Stopped ||
                         m.State != TimState.Canceled ||
                         m.State != TimState.Error ||
                         !m.Enable))
        {
            switch (message.State)
            {
                case TimState.Running:
                {
                    if (message.CancelOnDuration)
                    {
                        if (message.DeliveryStart + message.DeliveryDuration < DateTime.UtcNow)
                        {
                           message.Action = Action.Delete;
                           message.State = TimState.Canceling;
                           result.Add(message);
                        }
                        break; 
                    }
                    message.DeliveryStart = DateTime.UtcNow;
                    message.DeliveryDuration = TimeSpan.FromMinutes(1);
                    message.Action = Action.Update;
                    result.Add(message);
                    break;
                }
                case TimState.Canceling:
                    if (!message.CancelOnDuration &&
                        message.DeliveryStart + message.DeliveryDuration < DateTime.UtcNow)
                    {
                        message.Action = Action.Delete;
                        message.State = TimState.Stopped;
                        result.Add(message);
                    }
                    break;
            }
        }
        return result;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _timer.Start(RunAsync);
    }
}
