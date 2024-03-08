// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
namespace Econolite.Ode.Models.Tim;

public enum TimRowStatus
{
    Active = 1,
    NotInService = 2,
    NotReady = 3,
    CreateAndGo = 4,
    CreateAndWait = 5,
    Destroy = 6
}
