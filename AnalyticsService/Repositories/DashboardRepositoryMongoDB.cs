using AnalyticsService.Models;
using AnalyticsService.Repositories.Interfaces;
using MongoDB.Driver;

namespace AnalyticsService.Repositories;

public class DashboardRepositoryMongoDb(
    ILogger<DashboardRepositoryMongoDb> logger,
    IMongoDatabase mongoDatabase) : IDashboardRepository
{
    private readonly IMongoCollection<WorkoutSession> _collection =
        mongoDatabase.GetCollection<WorkoutSession>("WorkoutSessions");

    public async Task<List<WorkoutSession>> GetSessionsByMemberId(int memberId)
    {
        logger.LogDebug("GetSessionsByMemberId called for memberId: {MemberId}", memberId);
        try
        {
            var filter = Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId);
            return await _collection.Find(filter).ToListAsync();
        }
        catch (Exception e)
        {
            logger.LogError(e, "GetSessionsByMemberId failed for memberId: {MemberId}", memberId);
            return [];
        }
    }
}