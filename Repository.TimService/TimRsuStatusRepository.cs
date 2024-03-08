// SPDX-License-Identifier: MIT
// Copyright: 2023 Econolite Systems, Inc.
using System.Collections.Immutable;
using Econolite.Ode.Models.Tim;
using Econolite.Ode.Models.Tim.Db;
using Econolite.Ode.Models.Tim.Dto;
using Econolite.Ode.Persistence.Mongo.Context;
using Econolite.Ode.Persistence.Mongo.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Econolite.Ode.Repository.TimService;

public class TimRsuStatusRepository : GuidDocumentRepositoryBase<TimRsuStatus>, ITimRsuStatusRepository
{
    public TimRsuStatusRepository(IMongoContext context, ILogger<TimRsuStatusRepository> logger) : base(context, logger){}
    
    public async Task<IEnumerable<TimRsuStatus>> GetByBatchId(Guid batchId)
    {
        var filter = Builders<TimRsuStatus>.Filter.And(
            Builders<TimRsuStatus>.Filter.ElemMatch(s => s.Broadcastings, b => b.BatchId == batchId),
            Builders<TimRsuStatus>.Filter.Eq(t => t.Deleted, false));
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
    
    public async Task<IEnumerable<TimRsuStatus>> GetByIntersectionId(Guid id)
    {
        var filter = Builders<TimRsuStatus>.Filter.And(
            Builders<TimRsuStatus>.Filter.Eq(t => t.IntersectionId, id),
            Builders<TimRsuStatus>.Filter.Eq(t => t.Deleted, false));
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
    
    public async Task<IReadOnlyList<TimRsuStatus>> FindActive()
    {
        var filter = Builders<TimRsuStatus>.Filter.Eq(t => t.Deleted, false);
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
    
    public async Task<IReadOnlyList<TimRsuStatus>> Find(DateTime startDate, DateTime? endDate)
    {
        var filterStartDate = Builders<TimRsuStatus>.Filter.Gte(t => t.CreationDate, startDate);
        var filterEndDate = endDate.HasValue ? Builders<TimRsuStatus>.Filter.Lte(t => t.CreationDate, endDate.Value) : null;
        var filter = endDate.HasValue ? Builders<TimRsuStatus>.Filter.And(filterStartDate, filterEndDate) : filterStartDate;
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
}
