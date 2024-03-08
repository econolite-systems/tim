// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim;

public class DownstreamTarget
{
    public int Target { get; set; } = 1;
}

public class UpstreamTarget
{
    public int Target { get; set; } = 1;
}

public class RadiusTarget
{
    public Radius Target { get; set; } = new();
}

public class EntityTarget
{
    public Guid Target { get; set; } = Guid.Empty;
}

public class Radius
{
    public double Value { get; set; }
    public string Units { get; set; } = string.Empty;
}

public class BroadcastTargets
{
    
}

public class BroadcastTimMessage
{
    
}

public class BroadcastTim
{
    
}
