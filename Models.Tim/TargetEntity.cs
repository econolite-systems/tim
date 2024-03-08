// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim;

public class TargetEntity
{
    public Guid IntersectionId { get; set; } = Guid.Empty;
    public string IntersectionName { get; set; } = string.Empty;
    public Guid TargetId { get; set; } = Guid.Empty;
    public string TargetName { get; set; } = string.Empty;
    public bool IsIntersectionValid { get; set; }
    public bool IsTargetValid { get; set; }
    
    public double[] Location { get; set; } = Array.Empty<double>();
    public double[][][] Region { get; set; } = Array.Empty<double[][]>();
}
