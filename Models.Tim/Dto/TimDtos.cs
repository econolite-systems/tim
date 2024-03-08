// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.ComponentModel.DataAnnotations;
using Econolite.Ode.Models.Tim.Db;

namespace Econolite.Ode.Models.Tim.Dto;


public sealed record SendTimTextBatchDto(
    List<Guid> SignalIds,
    DateTime DeliveryStart,
    DateTime DeliveryEnd,
    [StringLength(maximumLength: 500, MinimumLength = 1)]
    string Message
);

public sealed record SendTimItisCodeBatchDto(
    List<Guid> SignalIds,
    DateTime DeliveryStart,
    DateTime DeliveryEnd,
    long Code
);

public sealed record TimDocumentDto(
    Guid Id,
    Guid BatchId,
    bool Deleted,
    TimState State,
    TimSource Source,
    Guid SignalId,
    DateTime CreationDate,
    DateTime DeliveryStart,
    DateTime DeliveryEnd,
    // Must be object to support polymorphic serialization for TimContents
    object Contents
);

public static class TimDocumentDtoAdapter
{
    public static TimDocumentDto AdaptToDto(this TimDocumentDto tim) =>
        new TimDocumentDto(
            Id: tim.Id,
            BatchId: tim.BatchId,
            Deleted: tim.Deleted,
            State: tim.State,
            Source: tim.Source,
            SignalId: tim.SignalId,
            CreationDate: tim.CreationDate,
            DeliveryStart: tim.DeliveryStart,
            DeliveryEnd: tim.DeliveryEnd,
            Contents: tim.Contents
        );
}

public sealed record TimTextContentsDto(
    string Message
);
