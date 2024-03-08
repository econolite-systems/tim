// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim.ItisCodes;
public class ItisCodeType
{
    public string Label { get; set; } = string.Empty;
    public ItisCode Value { get; set; } = ItisCode.None;
    public bool FireOnce { get; set; }
    public IEnumerable<string> MessageTypes { get; set; } = new List<string>();
    public DurationType DurationType { get; set; } = DurationType.None;
}

public enum DurationType
{
    None,
    Minutes,
    Hours,
    Days,
    Weeks
}

public class ItisCodeVehicleTravelingWrongWay : ItisCodeType
{
    public ItisCodeVehicleTravelingWrongWay()
    {
        this.Value = ItisCode.VehicleTravelingWrongWay;
        this.Label = $"Vehicle Traveling Wrong Way ({(int)this.Value})";
        this.FireOnce = true;
        this.DurationType = DurationType.Minutes;
        this.MessageTypes = new List<string> { "Information" };
    }
}

public class ItisCodeBlackIce : ItisCodeType
{
    public ItisCodeBlackIce()
    {
        this.Value = ItisCode.BlackIce;
        this.Label = $"Black Ice ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeIce : ItisCodeType
{
    public ItisCodeIce()
    {
        this.Value = ItisCode.Ice;
        this.Label = $"Ice ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeWhiteOut : ItisCodeType
{
    public ItisCodeWhiteOut()
    {
        this.Value = ItisCode.WhiteOut;
        this.Label = $"White Out ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeFog : ItisCodeType
{
    public ItisCodeFog()
    {
        this.Value = ItisCode.Fog;
        this.Label = $"Fog ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeDamagingHail : ItisCodeType
{
    public ItisCodeDamagingHail()
    {
        this.Value = ItisCode.DamagingHail;
        this.Label = $"Damaging Hail ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeFreezingRain : ItisCodeType
{
    public ItisCodeFreezingRain()
    {
        this.Value = ItisCode.FreezingRain;
        this.Label = $"Freezing Rain ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeVisibilityReduced : ItisCodeType
{
    public ItisCodeVisibilityReduced()
    {
        this.Value = ItisCode.VisibilityReduced;
        this.Label = $"Visibility Reduced ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeStrongWinds : ItisCodeType
{
    public ItisCodeStrongWinds()
    {
        this.Value = ItisCode.StrongWinds;
        this.Label = $"Strong Winds ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeBlizzard : ItisCodeType
{
    public ItisCodeBlizzard()
    {
        this.Value = ItisCode.Blizzard;
        this.Label = $"Blizzard ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeHeavySnow : ItisCodeType
{
    public ItisCodeHeavySnow()
    {
        this.Value = ItisCode.HeavySnow;
        this.Label = $"Heavy Snow ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeDangerOfHydroplaning : ItisCodeType
{
    public ItisCodeDangerOfHydroplaning()
    {
        this.Value = ItisCode.DangerOfHydroplaning;
        this.Label = $"Danger Of Hydroplaning ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeSnowOnRoadway : ItisCodeType
{
    public ItisCodeSnowOnRoadway()
    {
        this.Value = ItisCode.SnowOnRoadway;
        this.Label = $"Snow On Roadway ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeRainAndSnowMixed : ItisCodeType
{
    public ItisCodeRainAndSnowMixed()
    {
        this.Value = ItisCode.RainAndSnowMixed;
        this.Label = $"Rain And Snow Mixed ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeSnow : ItisCodeType
{
    public ItisCodeSnow()
    {
        this.Value = ItisCode.Snow;
        this.Label = $"Snow ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeHeavyRain : ItisCodeType
{
    public ItisCodeHeavyRain()
    {
        this.Value = ItisCode.HeavyRain;
        this.Label = $"Heavy Rain ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Alert" };
    }
}

public class ItisCodeTornado : ItisCodeType
{
    public ItisCodeTornado()
    {
        this.Value = ItisCode.Tornado;
        this.Label = $"Tornado ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Warning", "Watch" };
    }
}

public class ItisCodeSevereWeather : ItisCodeType
{
    public ItisCodeSevereWeather()
    {
        this.Value = ItisCode.SevereWeather;
        this.Label = $"Severe Weather ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Warning", "Watch" };
    }
}

public class ItisCodeWinterStorm : ItisCodeType
{
    public ItisCodeWinterStorm()
    {
        this.Value = ItisCode.WinterStorm;
        this.Label = $"Winter Storm ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Warning", "Watch" };
    }
}

public class ItisCodeIceStorm : ItisCodeType
{
    public ItisCodeIceStorm()
    {
        this.Value = ItisCode.IceStorm;
        this.Label = $"Ice Storm ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Warning", "Watch" };
    }
}

public class ItisCodeThunderstorms : ItisCodeType
{
    public ItisCodeThunderstorms()
    {
        this.Value = ItisCode.Thunderstorms;
        this.Label = $"Thunderstorms ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Warning", "Watch" };
    }
}

public class ItisCodeStoppedTraffic : ItisCodeType
{
    public ItisCodeStoppedTraffic()
    {
        this.Value = ItisCode.StoppedTraffic;
        this.Label = $"Stopped Traffic ({(int)this.Value})";
        this.FireOnce = true;
        this.MessageTypes = new List<string> { "Information" };
        this.DurationType = DurationType.Minutes;
    }
}

public class ItisCodeStopAndGoTraffic : ItisCodeType
{
    public ItisCodeStopAndGoTraffic()
    {
        this.Value = ItisCode.StopAndGoTraffic;
        this.Label = $"Stop And Go Traffic ({(int)this.Value})";
        this.FireOnce = true;
        this.MessageTypes = new List<string> { "Information" };
        this.DurationType = DurationType.Minutes;
    }
}

public class ItisCodeSlowTraffic : ItisCodeType
{
    public ItisCodeSlowTraffic()
    {
        this.Value = ItisCode.SlowTraffic;
        this.Label = $"Slow Traffic ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Information" };
    }
}

public class ItisCodeLongQueues : ItisCodeType
{
    public ItisCodeLongQueues()
    {
        this.Value = ItisCode.LongQueues;
        this.Label = $"Long Queues ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Information" };
    }
}

public class ItisCodeSpeedLimit : ItisCodeType
{
    public ItisCodeSpeedLimit()
    {
        this.Value = ItisCode.SpeedLimit;
        this.Label = $"Speed Limit ({(int)this.Value})";
        this.MessageTypes = new List<string> { "Information" };
    }
}

public class ItisCodePothole : ItisCodeType
{
    public ItisCodePothole()
    {
        this.Value = ItisCode.Pothole;
        this.Label = $"Pothole ({(int)this.Value})";
        this.FireOnce = true;
        this.MessageTypes = new List<string> { "Information" };
        this.DurationType = DurationType.Days;
    }
}

public class ItisCodeBumps : ItisCodeType
{
    public ItisCodeBumps()
    {
        this.Value = ItisCode.Bumps;
        this.Label = $"Bumps ({(int)this.Value})";
        this.FireOnce = true;
        this.MessageTypes = new List<string> { "Information" };
        this.DurationType = DurationType.Days;
    }
}

public static class ItisCodeExtensions
{
    public static IEnumerable<ItisCodeType> GetAllItisCodeTypes()
    {
        return new ItisCodeType[]
        {
            new ItisCodeVehicleTravelingWrongWay(),
            new ItisCodeBlackIce(),
            new ItisCodeIce(),
            new ItisCodeWhiteOut(),
            new ItisCodeFog(),
            new ItisCodeDamagingHail(),
            new ItisCodeFreezingRain(),
            new ItisCodeVisibilityReduced(),
            new ItisCodeStrongWinds(),
            new ItisCodeBlizzard(),
            new ItisCodeHeavySnow(),
            new ItisCodeDangerOfHydroplaning(),
            new ItisCodeSnowOnRoadway(),
            new ItisCodeRainAndSnowMixed(),
            new ItisCodeSnow(),
            new ItisCodeHeavyRain(),
            new ItisCodeTornado(),
            new ItisCodeSevereWeather(),
            new ItisCodeWinterStorm(),
            new ItisCodeIceStorm(),
            new ItisCodeThunderstorms(),
            new ItisCodeStoppedTraffic(),
            new ItisCodeStopAndGoTraffic(),
            new ItisCodeSlowTraffic(),
            new ItisCodeLongQueues(),
            new ItisCodeSpeedLimit(),
            new ItisCodePothole(),
            new ItisCodeBumps()
        };
    }
    
    public static ItisCodeType ToItisCodeType(this ItisCode itisCode)
    {
        return itisCode switch
        {
            ItisCode.Alert => new ItisCodeAlert(),
            ItisCode.Warning => new ItisCodeWarning(),
            ItisCode.Watch => new ItisCodeWatch(),
            ItisCode.AlertCanceled => new ItisCodeAlertCanceled(),
            ItisCode.WarningCanceled => new ItisCodeWarningCanceled(),
            ItisCode.WatchCanceled => new ItisCodeWatchCanceled(),
            ItisCode.VehicleTravelingWrongWay => new ItisCodeVehicleTravelingWrongWay(),
            ItisCode.BlackIce => new ItisCodeBlackIce(),
            ItisCode.Ice => new ItisCodeIce(),
            ItisCode.WhiteOut => new ItisCodeWhiteOut(),
            ItisCode.Fog => new ItisCodeFog(),
            ItisCode.DamagingHail => new ItisCodeDamagingHail(),
            ItisCode.FreezingRain => new ItisCodeFreezingRain(),
            ItisCode.VisibilityReduced => new ItisCodeVisibilityReduced(),
            ItisCode.StrongWinds => new ItisCodeStrongWinds(),
            ItisCode.Blizzard => new ItisCodeBlizzard(),
            ItisCode.HeavySnow => new ItisCodeHeavySnow(),
            ItisCode.DangerOfHydroplaning => new ItisCodeDangerOfHydroplaning(),
            ItisCode.SnowOnRoadway => new ItisCodeSnowOnRoadway(),
            ItisCode.RainAndSnowMixed => new ItisCodeRainAndSnowMixed(),
            ItisCode.Snow => new ItisCodeSnow(),
            ItisCode.HeavyRain => new ItisCodeHeavyRain(),
            ItisCode.Tornado => new ItisCodeTornado(),
            ItisCode.SevereWeather => new ItisCodeSevereWeather(),
            ItisCode.WinterStorm => new ItisCodeWinterStorm(),
            ItisCode.IceStorm => new ItisCodeIceStorm(),
            ItisCode.Thunderstorms => new ItisCodeThunderstorms(),
            ItisCode.StoppedTraffic => new ItisCodeStoppedTraffic(),
            ItisCode.StopAndGoTraffic => new ItisCodeStopAndGoTraffic(),
            ItisCode.SlowTraffic => new ItisCodeSlowTraffic(),
            ItisCode.LongQueues => new ItisCodeLongQueues(),
            ItisCode.SpeedLimit => new ItisCodeSpeedLimit(),
            ItisCode.Pothole => new ItisCodePothole(),
            ItisCode.Bumps => new ItisCodeBumps(),
            _ => throw new ArgumentOutOfRangeException(nameof(itisCode), itisCode, null)
        };
    }

    public static MessageType ToMessageType(this ItisCode code)
    {
        var itisCodeType = code.ToItisCodeType();
        return itisCodeType.ToMessageType();
    }
    
    private static MessageType ToMessageType(this ItisCodeType itisCodeType)
    {
        return itisCodeType.MessageTypes.FirstOrDefault() switch
        {
            "Alert" => MessageType.Alert,
            "Warning" => MessageType.Warning,
            "Watch" => MessageType.Watch,
            _ => MessageType.Information
        };
    }
}

public class ItisCodeWatch : ItisCodeType
{
    public ItisCodeWatch()
    {
        this.Value = ItisCode.Watch;
        this.Label = $"Watch ({(int)this.Value})";
        this.MessageTypes = Array.Empty<string>();
    }
}

public class ItisCodeWarning : ItisCodeType
{
    public ItisCodeWarning()
    {
        this.Value = ItisCode.Warning;
        this.Label = $"Warning ({(int)this.Value})";
        this.MessageTypes = Array.Empty<string>();
    }
}

public class ItisCodeAlert : ItisCodeType
{
    public ItisCodeAlert()
    {
        this.Value = ItisCode.Alert;
        this.Label = $"Alert ({(int)this.Value})";
        this.MessageTypes = Array.Empty<string>();
    }
}

public class ItisCodeWatchCanceled : ItisCodeType
{
    public ItisCodeWatchCanceled()
    {
        this.Value = ItisCode.WatchCanceled;
        this.Label = $"Watch Canceled ({(int)this.Value})";
        this.MessageTypes = Array.Empty<string>();
    }
}

public class ItisCodeWarningCanceled : ItisCodeType
{
    public ItisCodeWarningCanceled()
    {
        this.Value = ItisCode.WarningCanceled;
        this.Label = $"Warning Canceled ({(int)this.Value})";
        this.MessageTypes = Array.Empty<string>();
    }
}

public class ItisCodeAlertCanceled : ItisCodeType
{
    public ItisCodeAlertCanceled()
    {
        this.Value = ItisCode.AlertCanceled;
        this.Label = $"Alert Canceled({(int)this.Value})";
        this.MessageTypes = Array.Empty<string>();
    }
}
