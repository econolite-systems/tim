// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Text.Json;

namespace Econolite.Ode.Models.Tim.Messaging;

public class TimMessageParser
{
    public TimMessage Parse(string type, string data)
    {
        try
        {
            return type switch
            {
                nameof(SendTimMessage) => JsonSerializer.Deserialize<SendTimMessage>(data)!,
                nameof(UpdateTimTextBatch) => JsonSerializer.Deserialize<UpdateTimTextBatch>(data)!,
                nameof(DeleteTimMessage) => JsonSerializer.Deserialize<DeleteTimMessage>(data)!,
                nameof(DeleteTimMessageBatch) => JsonSerializer.Deserialize<DeleteTimMessageBatch>(data)!,
                _ => new UnknownTimMessage(type, data)
            };
        }
        catch (Exception ex)
        {
            return new NonParsableTimMessage(type, data, ex);
        }
    }
}
