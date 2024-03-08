using Econolite.Ode.Commands.Rsu.Messaging;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Models.Tim.ItisCodes;
using Econolite.Ode.Persistence.Common.Interfaces;

namespace Econolite.Ode.Models.Tim.Db;

public class TimRsuStatus : TimDocument
{
    public IEnumerable<TimDocument> Broadcastings { get; set; } = Array.Empty<TimDocument>();
}

public static class TimRsuStatusExtensions
{
    public static TimStatus ToStatus(this TimRsuStatus timDocument, IEnumerable<TargetEntity> targetEntities)
    {
        var targetEntity = targetEntities.FirstOrDefault(t => t.IntersectionId == timDocument.IntersectionId);
        return new TimStatus()
        {
            Id = timDocument.Id,
            BatchId = timDocument.Id,
            Intersection = targetEntity?.IntersectionName ?? string.Empty,
            Rsu = targetEntity?.TargetName ?? string.Empty,
            Status = Enum.GetName(timDocument.State) ?? "Pending",
            Message = timDocument.Payload.Select(itis => ((ItisCode)itis).ToItisCodeType().Label).Aggregate((a, b) => $"{a}, {b}") ?? string.Empty,
            DeliveryTime = timDocument.DeliveryStart
        };
    }
    
    public static TimRsuStatus ToTimRsuStatus(this TimDocument timDocument)
    {
        return new TimRsuStatus()
        {
            Id = timDocument.Id,
            IntersectionId = timDocument.IntersectionId,
            RsuId = timDocument.RsuId,
            State = timDocument.State,
            CreationDate = timDocument.CreationDate,
            EndDate = timDocument.EndDate, 
            CancelOnDuration = timDocument.CancelOnDuration,
            Action = timDocument.Action,
            Index = timDocument.Index,
            IsAlternating = timDocument.IsAlternating,
            DeliveryStart = timDocument.DeliveryStart,
            DeliveryDuration = timDocument.DeliveryDuration,
            Enable = timDocument.Enable,
            Payload = timDocument.Payload,
            Location = timDocument.Location,
            Region = timDocument.Region,
            ItisCode = timDocument.ItisCode,
            MessageType = timDocument.MessageType,
            Broadcastings = new[] {timDocument}
        };
    }
    
    public static TimRsuStatus AddTimDocument(this TimRsuStatus timRsuStatus, TimDocument timDocument)
    {
        var broadcastings = timRsuStatus.Broadcastings.ToList();
        broadcastings.Add(timDocument);
        timRsuStatus.Broadcastings = broadcastings;
        var start = timRsuStatus.Broadcastings.Min(t => t.DeliveryStart);
        var duration = timRsuStatus.Broadcastings.Max(t => t.DeliveryDuration);
        var end = timRsuStatus.Broadcastings.Where(b => b.EndDate.HasValue).Max(t => t.EndDate);
        timRsuStatus.DeliveryStart = start;
        timRsuStatus.DeliveryDuration = duration;
        timRsuStatus.EndDate = end;
        return timRsuStatus;
    }
    
    public static TimRsuStatus RemoveTimDocument(this TimRsuStatus timRsuStatus, TimDocument timDocument)
    {
        var broadcastings = timRsuStatus.Broadcastings.Where(t => t.Id != timDocument.Id).ToList();
        timRsuStatus.Broadcastings = broadcastings;
        if (!broadcastings.Any()) return timRsuStatus;
        
        var start = timRsuStatus.Broadcastings.Min(t => t.DeliveryStart);
        var duration = timRsuStatus.Broadcastings.Max(t => t.DeliveryDuration);
        var end = timRsuStatus.Broadcastings.Where(b => b.EndDate.HasValue).Max(t => t.EndDate);
        timRsuStatus.DeliveryStart = start;
        timRsuStatus.DeliveryDuration = duration;
        timRsuStatus.EndDate = end;

        return timRsuStatus;
    }
    
    public static TimCommandRequest ToRequest(this TimRsuStatus timDocument)
    {
        return new TimCommandRequest()
        {
            Action = timDocument.Action,
            Id = timDocument.Id,
            RsuId = timDocument.RsuId,
            Index = timDocument.Index,
            IsAlternating = timDocument.IsAlternating,
            DeliveryStart = timDocument.DeliveryStart,
            DeliveryDuration = timDocument.DeliveryDuration,
            Enable = timDocument.Enable,
            Payload = timDocument.Payload,
            TimeStamp = DateTime.UtcNow
        };
    }
}