// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Persistence.Common.Repository;

namespace Econolite.Ode.Repository.TimService;

public interface ITimRsuStatusRepository : IRepository<TimRsuStatus, Guid>
{
    Task<IReadOnlyList<TimRsuStatus>> FindActive();
    Task<IReadOnlyList<TimRsuStatus>> Find(DateTime startDate, DateTime? endDate);
    Task<IEnumerable<TimRsuStatus>> GetByBatchId(Guid batchId);
    Task<IEnumerable<TimRsuStatus>> GetByIntersectionId(Guid id);
}
