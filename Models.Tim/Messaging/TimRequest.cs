// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.ItisCodes;
using Econolite.Ode.Models.Tim.Messaging;

namespace Econolite.Ode.TimService.Models;

public class TimRequest
{
    public Guid Id { get; set; } = Guid.Empty;
    public bool Cancel { get; set; } = false;
    public MessageType MessageType { get; set; } = MessageType.Information;
    public ItisCode ItisCode { get; set; } = ItisCode.None;
    public TimTransmitMode TransmitMode { get; set; }
    public DurationType DurationType { get; set; } = DurationType.None;
    public int? Duration { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public TargetType TargetType { get; set; } = TargetType.None;
    public IEnumerable<Guid> Target { get; set; } = Array.Empty<Guid>();
    public IEnumerable<string> Parameters { get; set; } = Array.Empty<string>();
}

public static class TimRequestExtensions
{
    public static TimDocument ToTimDocument(this TimRequest request, TargetEntity targetEntity, Guid id = default, TimSource source = TimSource.ManualEntry)
    {
        var timMessageItems = request.MessageType.ToMessage(request.ItisCode);
        var timItisCodeType = request.ItisCode.ToItisCodeType();
        var documentId = id == default ? Guid.NewGuid() : id;
        
        return new TimDocument()
        {
            Id = documentId,
            BatchId = request.Id,
            Deleted = false,
            State = TimState.Pending,
            Source = source,
            IntersectionId = targetEntity.IntersectionId,
            RsuId = targetEntity.TargetId,
            CreationDate = DateTime.UtcNow,
            Action = request.Cancel ? Commands.Rsu.Messaging.Action.Delete : Commands.Rsu.Messaging.Action.Create,
            DeliveryStart = DateTime.UtcNow,
            DeliveryDuration = request.Duration.HasValue ? request.DurationType.ToTimeSpan(request.Duration.Value) : TimeSpan.FromMinutes(1),
            ItisCode = request.ItisCode,
            MessageType = request.MessageType,
            Payload = timMessageItems.Select(item => (int)item.ItisCode!).ToArray(),
            Enable = true,
            IsAlternating = request.TransmitMode == TimTransmitMode.Alternating,
            CancelOnDuration = timItisCodeType.FireOnce,
            Location = targetEntity.Location,
            Region = targetEntity.Region
        };
    }
}