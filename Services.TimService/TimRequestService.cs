// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Commands.Rsu.Messaging;
using Econolite.Ode.Messaging;
using Econolite.Ode.Messaging.Elements;
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Models.Tim.ItisCodes;
using Econolite.Ode.Models.Tim.Messaging;
using Econolite.Ode.Repository.TimService;
using Econolite.Ode.TimService.Models;
using Action = Econolite.Ode.Commands.Rsu.Messaging.Action;

namespace Econolite.Ode.Services.TimService;

public class TimRequestService : ITimRequestService
{
    private readonly ITimRsuStatusRepository _timRepository;
    private readonly ISink<RsuCommandRequest> _rsuCommandRequestSink;
    private readonly IMessageFactory<Guid, TimMessage> _messageFactory;
    private readonly ITargetEntityResolver _targetEntityResolver;

    public TimRequestService(
        ITimRsuStatusRepository timRepository,
        ISink<RsuCommandRequest> rsuCommandRequestSink,
        IMessageFactory<Guid, TimMessage> messageFactory,
        ITargetEntityResolver targetEntityResolver
    )
    {
        _timRepository = timRepository;
        _rsuCommandRequestSink = rsuCommandRequestSink;
        _messageFactory = messageFactory;
        _targetEntityResolver = targetEntityResolver;
    }

    public async Task<IEnumerable<TimRsuStatus>> GetAllAsync()
    {
        return await _timRepository.FindActive();
    }
    
    public async Task<IEnumerable<TimStatus>> GetStatusAsync()
    {
        var result = new List<TimStatus>();
        var documents = (await GetAllAsync()).ToArray();
        if (!documents.Any()) return result;
  
        var targetEntities = await _targetEntityResolver.GetTargetEntities(documents.Select(d => d.IntersectionId).Distinct());
        result = documents.Select(d => d.ToStatus(targetEntities)).ToList();
        return result;
    }

    public async Task<IEnumerable<TimRsuStatus>> GetBatchAsync(Guid timId)
    {
        return await _timRepository.GetByBatchId(timId);
    }

    public async Task<TimRequest> SendRequest(TimRequest request, TimSource source = TimSource.ManualEntry)
    {
        if (request.Cancel)
        {
            await CancelRequest(request.Id);
            return request;
        }
        
        var targetEntities = await _targetEntityResolver.ResolveTargetEntities(request);
        var timMessageItems = request.MessageType.ToMessage(request.ItisCode);
        var timItisCodeType = request.ItisCode.ToItisCodeType();
        foreach (var targetEntity in targetEntities.ToArray())
        {
            var id = Guid.NewGuid();

            var timCommandRequest = new TimCommandRequest()
            {
                Action = Action.Create,
                Id = id,
                RsuId = targetEntity.TargetId,
                IsAlternating = request.TransmitMode == TimTransmitMode.Alternating,
                DeliveryStart = DateTime.UtcNow,
                DeliveryDuration = request.Duration.HasValue ? request.DurationType.ToTimeSpan(request.Duration.Value) : TimeSpan.FromMinutes(1),
                Enable = true,
                Payload = timMessageItems.Select(item => (int)item.ItisCode!).ToArray(),
                TimeStamp = DateTime.UtcNow,
                Location = targetEntity.Location,
                Region = targetEntity.Region
            };
            
            var timDocument = new TimDocument()
            {
                Id = id,
                BatchId = request.Id,
                Deleted = false,
                State = TimState.Pending,
                Source = source,
                IntersectionId = targetEntity.IntersectionId,
                RsuId = targetEntity.TargetId,
                CreationDate = DateTime.UtcNow,
                Action = Action.Create,
                DeliveryStart = timCommandRequest.DeliveryStart,
                DeliveryDuration = timCommandRequest.DeliveryDuration,
                ItisCode = request.ItisCode,
                MessageType = request.MessageType,
                Payload = timCommandRequest.Payload,
                Enable = timCommandRequest.Enable,
                IsAlternating = timCommandRequest.IsAlternating,
                CancelOnDuration = timItisCodeType.FireOnce,
                Location = timCommandRequest.Location,
                Region = timCommandRequest.Region
            };
            
            var timRsuStatuses = await _timRepository.GetByIntersectionId(targetEntity.IntersectionId);
            var timRsuStatus = timRsuStatuses.FirstOrDefault(t => t.ItisCode == timDocument.ItisCode && t.MessageType == timDocument.MessageType);
            if (timRsuStatus is not null)
            {
                timRsuStatus.AddTimDocument(timDocument);
                _timRepository.Update(timRsuStatus);
            }
            else
            {
                timRsuStatus = timDocument.ToTimRsuStatus();
                _timRepository.Add(timRsuStatus);
            }
            
            await _rsuCommandRequestSink.SinkAsync(timCommandRequest.Id, timCommandRequest, CancellationToken.None);
        }
        var (success, errors) = await _timRepository.DbContext.SaveChangesAsync();
        return request;
    }

    public async Task SendUpdate(IEnumerable<TimRsuStatus> requests, CancellationToken stoppingToken = default)
    {
        foreach (var request in requests.ToArray())
        {
            _timRepository.Update(request);
            await _rsuCommandRequestSink.SinkAsync(request.Id, request.ToRequest(), stoppingToken);
        }
        
        await _timRepository.DbContext.SaveChangesAsync();
    }
    
    public async Task CancelRequest(Guid requestId)
    {
        var tims = await _timRepository.GetByBatchId(requestId);
        foreach (var tim in tims.ToArray())
        {
            if (!tim.Payload.Any()) continue;

            var document = tim.Broadcastings.FirstOrDefault(t => t.BatchId == requestId);
            if (document is null) continue;

            tim.RemoveTimDocument(document);
            
            if (!tim.Broadcastings.Any())
            {
                if (tim.MessageType is MessageType.Information)
                {
                    tim.State = TimState.Canceling;
                    tim.Action = Action.Delete;
                }
                else
                {
                    var cancelMessage = tim.MessageType.ToCancelMessage(tim.ItisCode);
                    tim.State = TimState.Canceling;
                    tim.Action = Action.Update;
                    tim.Payload = cancelMessage.Select(item => (int) item.ItisCode!).ToArray();
                }
                
                await _rsuCommandRequestSink.SinkAsync(tim.Id, tim.ToRequest(), CancellationToken.None);
            }
            
            _timRepository.Update(tim);
        }
        
        await _timRepository.DbContext.SaveChangesAsync();
    }

    public async Task CancelTim(Guid timId)
    {
        var tim = await _timRepository.GetByIdAsync(timId);
        if (tim is null || !tim.Payload.Any()) return;
        
        if (tim.MessageType is MessageType.Information)
        {
            tim.State = TimState.Canceling;
            tim.Action = Action.Delete;
        }
        else
        {
            var cancelMessage = tim.MessageType.ToCancelMessage(tim.ItisCode);
            tim.State = TimState.Canceling;
            tim.Action = Action.Update;
            tim.Payload = cancelMessage.Select(item => (int)item.ItisCode!).ToArray(); 
        }
        
        _timRepository.Update(tim);
        await _rsuCommandRequestSink.SinkAsync(tim.Id, tim.ToRequest(), CancellationToken.None);
        await _timRepository.DbContext.SaveChangesAsync();
    }

    public async Task DeleteTim(Guid timId)
    {
        await CancelTim(timId);
        var tim = await _timRepository.GetByIdAsync(timId);
        if (tim is null) return;
        tim.EndDate ??= DateTime.UtcNow;
        tim.Deleted = true;
        _timRepository.Update(tim);
        await _timRepository.DbContext.SaveChangesAsync();
    }

    public async Task DeleteBatch(Guid batchId)
    {
        var tims = await _timRepository.GetByBatchId(batchId);
        foreach (var tim in tims.ToArray())
        {
            await DeleteTim(tim.Id);
        }
        
        await _timRepository.DbContext.SaveChangesAsync();
    }
}
