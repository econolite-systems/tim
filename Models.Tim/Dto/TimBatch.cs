// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim.Dto;

public sealed record TimBatchDto(
    Guid BatchId,
    IEnumerable<Guid> TimIds
);
