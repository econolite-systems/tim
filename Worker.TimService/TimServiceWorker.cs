// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Api.JpoOdeTim;
using Econolite.Ode.Messaging;
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.Monitoring.Events;
using Econolite.Ode.Monitoring.Events.Extensions;
using Econolite.Ode.Monitoring.Metrics;
using Econolite.Ode.Repository.TimService;

namespace Econolite.Ode.Worker.TimService;

public class TimServiceWorker : BackgroundService
{
    private readonly ILogger<TimServiceWorker> _logger;
    private readonly ITimRepository _timRepository;
    //private readonly ISignalRepository _signalRepository;
    private readonly IConsumer<Guid, TimMessage> _consumer;
    private readonly JpoOdeTimClient _timClient;
    private readonly UserEventFactory _userEventFactory;
    private readonly IMetricsCounter _loopCounter;

    public TimServiceWorker(
        ILogger<TimServiceWorker> logger,
        IConfiguration config,
        ITimRepository timRepository,
        //ISignalRepository signalRepository,
        IConsumer<Guid, TimMessage> consumer,
        IMetricsFactory metricsFactory,
        UserEventFactory userEventFactory
    )
    {
        _logger = logger;
        _timRepository = timRepository;
        //_signalRepository = signalRepository;
        _consumer = consumer;
        _userEventFactory = userEventFactory;

        var jpoOdeUri = config.GetConnectionString("JpoOde") ?? throw new NullReferenceException("ConnectionStrings:JpoOde missing in config.");
        _timClient = new JpoOdeTimClient(new Uri(jpoOdeUri));

        _logger.LogDebug("Using JPO ODE URL: '{}'", jpoOdeUri);

        var topic = config["Topics:Tim"] ?? throw new NullReferenceException("Topics:Tim missing in config.");

        _consumer.Subscribe(topic);
        _logger.LogInformation("Subscribed to topic: {}", topic);

        _loopCounter = metricsFactory.GetMetricsCounter("Worker");
    }

    // private static object GetMessageContents(TimDocument message)
    // {
    //     return message.Contents switch
    //     {
    //         TextTimContents text => text.Message,
    //         ItisCodeTimContents itis => (int)itis.Code,
    //         _ => throw new ArgumentOutOfRangeException()
    //     };
    // }

    private async Task HandleSendMessageAsync(SendTimMessage sendTim)
    {
        _logger.LogDebug("Sending TIM text message: {}", sendTim);

        // var message = await _timRepository.GetById(sendTim.MessageId);
        // if (message is null)
        // {
        //     _logger.LogError("Cannot send unknown TIM message: {}", sendTim.MessageId);
        //     return;
        // }
        //
        // var signal = await _signalRepository.GetByIdAsync(message.SignalId);
        // if (signal is null)
        // {
        //     _logger.LogError("Cannot send TIM to signal that doesn't exist: {}", message.SignalId);
        //     return;
        // }
        //
        // var rsuConfig = signal.Rsu;
        // if (rsuConfig is null)
        // {
        //     _logger.LogError("Cannot send TIM to signal with no RSU configured: {}", message.SignalId);
        //     return;
        // }
        //
        // var rsuIndex = 0;
        // var rsuId = 0;
        // var messageId = 0;
        // var mode = TimTransmitMode.Continuous;
        // var channel = 172;
        // var interval = 1000;
        // var enable = 1;
        // var status = TimRowStatus.CreateAndGo;
        //
        // var now = DateTime.UtcNow.ToString("o");
        // var deliveryStart = message.DeliveryStart.ToUniversalTime().ToString("o");
        // var deliveryEnd = message.DeliveryEnd.ToUniversalTime().ToString("o");
        //
        // await _timClient.Create(new JpoOdeTimPost(
        //         new JpoOdeTimRequest(
        //             Rsus: new List<JpoOdeRsu>
        //             {
        //                 new JpoOdeRsu(
        //                     RsuIndex: rsuIndex,
        //                     RsuTarget: rsuConfig.IPAddress,
        //                     RsuUsername: rsuConfig.Username,
        //                     RsuPassword: rsuConfig.Password,
        //                     RsuRetries: 1,
        //                     RsuTimeout: 1000
        //                 )
        //             },
        //             Snmp: new JpoOdeSnmp(
        //                 RsuId: rsuId.ToString(),
        //                 MsgId: messageId.ToString(),
        //                 Mode: ((int)mode).ToString(),
        //                 Channel: channel.ToString(),
        //                 Interval: interval.ToString(),
        //                 DeliveryStart: deliveryStart,
        //                 DeliveryStop: deliveryEnd,
        //                 Enable: enable,
        //                 Status: ((int)status).ToString()
        //             ),
        //             Sdw: null
        //         ),
        //         Tim: new JpoOdeTimMessages(
        //             MsgCnt: 1,
        //             TimeStamp: now,
        //             PacketId: null,
        //             UrlB: null,
        //             DataFrames: new List<JpoOdeDataFrame>()
        //             {
        //                 new JpoOdeDataFrame
        //                 (
        //                     SspTimRights: 0,
        //                     // One of the following TravelerInfoType enumeration constants: unknown, advisory, roadSignage, commercialSignage
        //                     FrameType: "advisory",
        //                     MsgId: new JpoOdeMessageId
        //                     (
        //                         RoadSignId: new JpoOideRoadSignId(
        //                             Position: new JpoOideRoadSignIdPosition(0.0m, 0.0m, 0.0m),
        //                             ViewAngle: "1010101010101010",
        //                             MutcdCode: null,
        //                             Crc: null
        //                         ),
        //                         FurtherInfoId: null
        //                     ),
        //                     StartDateTime: deliveryStart,
        //                     // Range [0..3200], message duration time, in minutes. A value of 32000 means forever.
        //                     DurationTime: 1000,
        //                     Priority: 1,
        //                     SspLocationRights: 3,
        //                     Regions: new List<JpoOdeRegion>
        //                     {
        //                         new JpoOdeRegion
        //                         (
        //                             Name: null,
        //                             RegulatorID: null,
        //                             SegmentID: null,
        //                             AnchorPosition: null,
        //                             LaneWidth: null,
        //                             Directionality: null,
        //                             ClosedPath: null,
        //                             Direction: null,
        //                             Description: null
        //                         )
        //                     },
        //                     SspMsgType: 2,
        //                     SspMsgContent: 3,
        //
        //                     // Part III content type, one of "Advisory", "Work Zone", "Generic Signage", "Speed Limit", or "Exit Service".
        //                     Content: "Advisory",
        //                     Items: new List<object>
        //                     {
        //                         GetMessageContents(message)
        //                     },
        //                     Url: null
        //                 )
        //             }
        //         )
        //     )
        // );
        //
        // await _timRepository.UpdateBatchState(sendTim.MessageId, TimState.Sent);
    }

    private Task HandleUpdateMessageAsync(UpdateTimTextBatch update)
    {
        _logger.LogDebug("Updating TIM batch message: {}", update);
        throw new NotImplementedException();
    }

    private async Task HandleDeleteMessageAsync(DeleteTimMessage deleteTim)
    {
        _logger.LogDebug("Deleting TIM message: {}", deleteTim);
        throw new NotImplementedException();
    }

    private async Task HandleDeleteBatchAsync(DeleteTimMessageBatch deleteBatch)
    {
        _logger.LogDebug("Deleting TIM batch message: {}", deleteBatch);
        throw new NotImplementedException();
    }

    private Task HandleUnknownAsync(UnknownTimMessage unknown)
    {
        _logger.LogError("Received unknown message: {}", unknown);
        return Task.CompletedTask;
    }

    private Task HandleNonParsableAsync(NonParsableTimMessage nonParsable)
    {
        _logger.LogError("Received non-parsable message: {}", nonParsable);
        return Task.CompletedTask;
    }

    private Task HandleUnhandledAsync(TimMessage unhandled)
    {
        _logger.LogError("Unhandled TIM message type: {}", unhandled);
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(async () =>
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume(stoppingToken);

                    try
                    {
                        var task = result.Value switch
                        {
                            SendTimMessage sendMessage => HandleSendMessageAsync(sendMessage),
                            UpdateTimTextBatch update => HandleUpdateMessageAsync(update),
                            DeleteTimMessage deleteMessage => HandleDeleteMessageAsync(deleteMessage),
                            DeleteTimMessageBatch deleteBatch => HandleDeleteBatchAsync(deleteBatch),
                            UnknownTimMessage unknown => HandleUnknownAsync(unknown),
                            NonParsableTimMessage nonParsable => HandleNonParsableAsync(nonParsable),
                            _ => HandleUnhandledAsync(result.Value)
                        };

                        await task;
                        _consumer.Complete(result);

                        _loopCounter.Increment();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Uncaught exception while processing message: {}", result.Value);

                        _logger.ExposeUserEvent(_userEventFactory.BuildUserEvent(EventLevel.Error, string.Format("Uncaught exception while processing message: {0}", result.Value)));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker.TimService stopping");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, "Worker.TimService stopping due to uncaught exception");
            }
        }, stoppingToken);
    }
}
