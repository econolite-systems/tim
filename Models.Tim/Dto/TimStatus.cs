// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim.Dto;

public class TimStatus
{
    public Guid Id { get; set; } = Guid.Empty;
    public Guid BatchId { get; set; } = Guid.Empty;
    public string Intersection { get; set; } = string.Empty;
    public string Rsu { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime DeliveryTime { get; set; }
}
