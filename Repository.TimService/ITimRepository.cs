// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Persistence.Common.Repository;

namespace Econolite.Ode.Repository.TimService;

public interface ITimRepository : IRepository<TimDocument, Guid>
{
    Task<IReadOnlyList<TimDocument>> FindActive();
    Task<IReadOnlyList<TimDocument>> Find(DateTime startDate, DateTime? endDate);
    Task<IEnumerable<TimDocument>> GetByBatchId(Guid batchId);
}
