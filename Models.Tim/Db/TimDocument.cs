// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Commands.Rsu.Messaging;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Models.Tim.ItisCodes;
using Econolite.Ode.Persistence.Common.Interfaces;
using Action = Econolite.Ode.Commands.Rsu.Messaging.Action;


namespace Econolite.Ode.Models.Tim.Db;

public enum TimState
{
    Pending,
    Canceling,
    Canceled,
    Running,
    Stopped,
    Error
}

public class TimDocument : IIndexedEntity<Guid>
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid IntersectionId { get; set; }
    public Guid RsuId { get; set; }
    public bool Deleted { get; set; }
    public TimState State { get; set; }
    public TimSource Source { get; set; }
    public DateTime CreationDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Error { get; set; } = string.Empty;
    public Action Action { get; set; } = Action.Create;
    public int? Index { get; set; }
    public bool IsAlternating { get; set; }
    public DateTime DeliveryStart { get; set; }
    public TimeSpan DeliveryDuration { get; set; } = TimeSpan.FromMinutes(5);
    public bool Enable { get; set; }
    public ItisCode ItisCode { get; set; } = ItisCode.None;
    public MessageType MessageType { get; set; } = MessageType.Information;
    public int[] Payload { get; set; } = Array.Empty<int>();
    public double[] Location { get; set; } = Array.Empty<double>();
    public double[][][] Region { get; set; } = Array.Empty<double[][]>();
    public bool CancelOnDuration { get; set; } = false;
}

public static class TimDocumentExtensions
{
    public static TimCommandRequest ToRequest(this TimDocument timDocument)
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
    
    public static TimStatus ToStatus(this TimDocument timDocument, IEnumerable<TargetEntity> targetEntities)
    {
        var targetEntity = targetEntities.FirstOrDefault(t => t.IntersectionId == timDocument.IntersectionId);
        return new TimStatus()
        {
            Id = timDocument.Id,
            BatchId = timDocument.BatchId,
            Intersection = targetEntity?.IntersectionName ?? string.Empty,
            Rsu = targetEntity?.TargetName ?? string.Empty,
            Status = Enum.GetName(timDocument.State) ?? "Pending",
            Message = timDocument.Payload.Select(itis => ((ItisCode)itis).ToItisCodeType().Label).Aggregate((a, b) => $"{a}, {b}") ?? string.Empty,
            DeliveryTime = timDocument.DeliveryStart
        };
    }
    
    public static TimeSpan ToTimeSpan(this DurationType type, int duration)
    {
        return type switch
        {
            DurationType.Minutes => TimeSpan.FromMinutes(duration),
            DurationType.Hours => TimeSpan.FromHours(duration),
            DurationType.Days => TimeSpan.FromDays(duration),
            DurationType.Weeks => TimeSpan.FromDays(duration * 7),
            _ => TimeSpan.FromMinutes(duration)
        };
    }
}
