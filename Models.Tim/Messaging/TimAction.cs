// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Tim.ItisCodes;

namespace Econolite.Ode.Models.Tim.Messaging;

public class TimActionSet
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public TimAction Action { get; set; } = new();
}

public class TimAction
{
    public Guid Id { get; set; } = Guid.Empty;
    public string ActionType { get; set; } = string.Empty;
    public ItisCode ItisCode { get; set; } = ItisCode.None;
    public TargetType TargetType { get; set; } = TargetType.None;
    public DurationType DurationType { get; set; } = DurationType.Minutes;
    public int Duration { get; set; } = 5;
    public IEnumerable<Guid> Target { get; set; } = Array.Empty<Guid>();
    public IEnumerable<string> Parameters { get; set; } = Array.Empty<string>();
}

public enum TargetType
{
    None,
    Downstream,
    Upstream,
    Radius,
    Target
}

public static class TargetTypeExtensions
{
    public static string ToTargetTypeString(this TargetType targetType)
    {
        return targetType switch
        {
            TargetType.None => "None",
            TargetType.Downstream => "Downstream",
            TargetType.Upstream => "Upstream",
            TargetType.Radius => "Radius",
            TargetType.Target => "Target",
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };
    }
    
    public static TargetType ToTargetType(this string targetType)
    {
        return targetType.ToLower() switch
        {
            "none" => TargetType.None,
            "downstream" => TargetType.Downstream,
            "upstream" => TargetType.Upstream,
            "radius" => TargetType.Radius,
            "target" => TargetType.Target,
            _ => throw new ArgumentOutOfRangeException(nameof(targetType), targetType, null)
        };
    }
}
