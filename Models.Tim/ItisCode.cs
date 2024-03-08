// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Collections.Immutable;
using Econolite.Ode.Models.Tim.ItisCodes;

namespace Econolite.Ode.Models.Tim;

public class TimMessageItem
{
    public bool IsItisCode {
        get
        {
            return ItisCode != null;
        }
    }
    
    public bool IsText
    {
        get
        {
            return Text != null;
        }
    }
    
    public string? Text { get; set; }
    public ItisCode? ItisCode { get; set; }
}

public enum MessageType
{
    Information,
    Alert,
    Warning,
    Watch
}

public enum ItisCode
{
    None = 0,
    VehicleTravelingWrongWay = 1793,
    BlackIce = 5908,
    Ice = 5906,
    StrongWindsHaveEased = 5246,
    VisibilityImproved = 5500,
    PavementConditionsCleared = 6015,
    WarningCanceled = 7034,
    WatchCanceled = 7035,
    AlertCanceled = 7036,
    Alert = 6916,
    WhiteOut = 5384,
    Fog = 5378,
    DamagingHail = 4879,
    FreezingRain = 5911,
    VisibilityReduced = 5383,
    StrongWinds = 5127,
    Blizzard = 4866,
    HeavySnow = 4867,
    DangerOfHydroplaning = 5894,
    SnowOnRoadway = 5916,
    RainAndSnowMixed = 4877,
    Snow = 4868,
    HeavyRain = 4884,
    Warning = 6915,
    Tornado = 5121,
    SevereWeather = 4865,
    WinterStorm = 4871,
    IceStorm = 4875,
    Thunderstorms = 4881,
    StoppedTraffic = 257,
    StopAndGoTraffic = 258,
    Watch = 6914,
    SlowTraffic = 259,
    LongQueues = 262,
    SpeedLimit = 268,
    Pothole = 1300,
    Bumps = 1053,
}

public class CancelMessage
{
    public ItisCode ItisCode { get; set; } = ItisCode.AlertCanceled;
    public List<ItisCode> ItisCodes { get; set; } = new List<ItisCode>()
    {
        ItisCode.StrongWindsHaveEased,
        ItisCode.VisibilityImproved,
        ItisCode.PavementConditionsCleared,
        ItisCode.WarningCanceled,
        ItisCode.WatchCanceled,
        ItisCode.AlertCanceled,
    };
}

public abstract class TimMessageBuilder
{
    protected abstract string MessageType { get; }
    protected abstract ItisCode Code { get; }
    protected abstract ItisCode CancelCode { get; }
    protected abstract List<ItisCode> ItisCodes { get; }

    public TimMessageItem[] BuildMessage(ItisCode code)
    {
        if (!ItisCodes.Contains(code))
        {
            throw new Exception($"Itis Code {code} not found for {MessageType} Message");
        }
        
        if (Code == ItisCode.None)
        {
            return new[] { code.ToTimMessageItem() };
        }
        
        return new TimMessageItem[]
        {
            Code.ToTimMessageItem(), code.ToTimMessageItem()
        };
    }
    
    public TimMessageItem[] BuildCancelMessage(ItisCode code)
    {
        if (!ItisCodes.Contains(code))
        {
            throw new Exception($"Itis Code {code} not found for {MessageType} Message");
        }

        if (CancelCode == ItisCode.None)
        {
            return new[] { code.ToTimMessageItem() };
        }
        
        return new []
        {
            CancelCode.ToTimMessageItem(), code.ToTimMessageItem()
        };
    }
}

public class InformationMessage : TimMessageBuilder
{
    protected override string MessageType => "Information";
    protected override ItisCode Code => ItisCode.None;
    protected override ItisCode CancelCode => ItisCode.None;
    protected override List<ItisCode> ItisCodes =>
        new()
        {
            ItisCode.VehicleTravelingWrongWay,
            ItisCode.StoppedTraffic,
            ItisCode.StopAndGoTraffic,
            ItisCode.SlowTraffic,
            ItisCode.LongQueues,
            ItisCode.SpeedLimit,
            ItisCode.Pothole,
            ItisCode.Bumps,
        };
    public TimMessageItem[] BuildMessageWithText(ItisCode code, string? text)
    {
        if (!ItisCodes.Contains(code))
        {
            throw new Exception($"Itis Code {code} not found for {MessageType} Message");
        }
        
        return new[] { code.ToTimMessageItem(), text.ToTimMessageItem() };
    }
}

public class AlertMessage : TimMessageBuilder
{
    protected override string MessageType => "Alert";
    protected override ItisCode Code => ItisCode.Alert;
    protected override ItisCode CancelCode => ItisCode.AlertCanceled;

    protected override List<ItisCode> ItisCodes =>
        new()
        {
            ItisCode.BlackIce,
            ItisCode.Ice,
            ItisCode.WhiteOut,
            ItisCode.Fog,
            ItisCode.DamagingHail,
            ItisCode.FreezingRain,
            ItisCode.VisibilityReduced,
            ItisCode.StrongWinds,
            ItisCode.Blizzard,
            ItisCode.HeavySnow,
            ItisCode.DangerOfHydroplaning,
            ItisCode.SnowOnRoadway,
            ItisCode.RainAndSnowMixed,
            ItisCode.Snow,
            ItisCode.HeavyRain,
        };
}

public class WarningMessage : TimMessageBuilder
{
    protected override string MessageType => "Warning";
    protected override ItisCode Code => ItisCode.Warning;
    protected override ItisCode CancelCode => ItisCode.WarningCanceled;

    protected override List<ItisCode> ItisCodes =>
        new()
        {
            ItisCode.Tornado,
            ItisCode.SevereWeather,
            ItisCode.WinterStorm,
            ItisCode.IceStorm,
            ItisCode.Thunderstorms,
        };
}

public class WatchMessage : TimMessageBuilder
{
    protected override string MessageType => "Watch";
    protected override ItisCode Code => ItisCode.Watch;
    protected override ItisCode CancelCode => ItisCode.WatchCanceled;

    protected override List<ItisCode> ItisCodes =>
        new()
        {
            ItisCode.Tornado,
            ItisCode.SevereWeather,
            ItisCode.WinterStorm,
            ItisCode.IceStorm,
            ItisCode.Thunderstorms,
        };
}

public static class TimMessageContentBuilder
{
    public static TimMessageItem[] ToCancelMessage(this MessageType type, ItisCode code, string? text = null)
    {
        return BuildCancelMessage(type, code);
    }
    
    public static TimMessageItem[] ToMessage(this MessageType type, ItisCode code, string? text = null)
    {
        return BuildMessage(type, code, text);
    }
    
    public static TimMessageItem[] BuildMessage(MessageType messageType, ItisCode code, string? text = null)
    {
        if (messageType == MessageType.Alert)
        {
            return new AlertMessage().BuildMessage(code);
        }
        
        if (messageType == MessageType.Warning)
        {
            return new WarningMessage().BuildMessage(code);
        }
        
        if (messageType == MessageType.Watch)
        {
            return new WatchMessage().BuildMessage(code);
        }
        
        if (messageType == MessageType.Information)
        {
            return text != null
                ? new InformationMessage().BuildMessageWithText(code, text)
                : new InformationMessage().BuildMessage(code);
        }
        
        throw new Exception($"Message Type {messageType.ToString()} not found for Itis Code {code}");
    }

    public static TimMessageItem[] BuildCancelMessage(MessageType messageType, ItisCode code)
    {
        if (messageType == MessageType.Alert)
        {
            return new AlertMessage().BuildCancelMessage(code);
        }
        
        if (messageType == MessageType.Warning)
        {
            return new WarningMessage().BuildCancelMessage(code);
        }
        
        if (messageType == MessageType.Watch)
        {
            return new WatchMessage().BuildCancelMessage(code);
        }
        
        if (messageType == MessageType.Information)
        {
            return new InformationMessage().BuildMessage(code);
        }
        
        throw new Exception($"Message Type {messageType.ToString()} not found for Itis Code {code}");
    }
}

public static class TimMessageExtensions
{
    public static ItisCode ToItisCode(this string code)
    {
        return Enum.Parse<ItisCode>(code);
    }

    public static MessageType ToMessageType(this string type)
    {
        return string.IsNullOrEmpty(type) ? MessageType.Information : Enum.Parse<MessageType>(type);
    }
    
    public static TimTransmitMode ToTimTransmitMode(this string mode)
    {
        return string.IsNullOrEmpty(mode) ? TimTransmitMode.Alternating : Enum.Parse<TimTransmitMode>(mode);
    }
    
    public static bool IsAlernating(this string type)
    {
        return type.ToLower() == "alternating";
    }
    
    public static DurationType ToDurationType(this string type)
    {
        return string.IsNullOrEmpty(type) ? DurationType.None : Enum.Parse<DurationType>(type);
    }
    
    public static TimMessageItem ToTimMessageItem(this string? text)
    {
        return new TimMessageItem() 
        {
            Text = text
        };
    }
    public static TimMessageItem ToTimMessageItem(this ItisCode code)
    { 
        return new TimMessageItem() 
        {
            ItisCode = code
        };
    }
    
    public static TimMessageItem[] ToTimMessageItems(this IEnumerable<ItisCode> codes)
    {
        return codes.Select(x => x.ToTimMessageItem()).ToArray();
    }
}
