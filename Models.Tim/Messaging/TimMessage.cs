// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim.Messaging;

public abstract record TimMessage;

public sealed record UnknownTimMessage(string Type, string Data) : TimMessage;

public sealed record NonParsableTimMessage(string Type, string Data, Exception Exception) : TimMessage;

public sealed record SendTimMessage(Guid MessageId) : TimMessage;

public sealed record UpdateTimTextBatch(Guid BatchId, string Message) : TimMessage;

public sealed record DeleteTimMessage(Guid TimId) : TimMessage;

public sealed record DeleteTimMessageBatch(Guid BatchId) : TimMessage;
