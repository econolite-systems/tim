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

public class TimRepository : GuidDocumentRepositoryBase<TimDocument>, ITimRepository
{
    public TimRepository(IMongoContext context, ILogger<TimRepository> logger) : base(context, logger){}
    
    public async Task<IEnumerable<TimDocument>> GetByBatchId(Guid batchId)
    {
        var filter = Builders<TimDocument>.Filter.Eq(t => t.BatchId, batchId);
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
    
    public async Task<IReadOnlyList<TimDocument>> FindActive()
    {
        var filter = Builders<TimDocument>.Filter.Eq(t => t.Deleted, false);
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
    
    public async Task<IReadOnlyList<TimDocument>> Find(DateTime startDate, DateTime? endDate)
    {
        var filterStartDate = Builders<TimDocument>.Filter.Gte(t => t.CreationDate, startDate);
        var filterEndDate = endDate.HasValue ? Builders<TimDocument>.Filter.Lte(t => t.CreationDate, endDate.Value) : null;
        var filter = endDate.HasValue ? Builders<TimDocument>.Filter.And(filterStartDate, filterEndDate) : filterStartDate;
        var result = await ExecuteDbSetFuncAsync(collection => collection.FindAsync(filter));
        return await result.ToListAsync();
    }
}
