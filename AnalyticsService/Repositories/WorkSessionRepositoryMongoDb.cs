using AnalyticsService.Models;
using MongoDB.Driver;

namespace AnalyticsService.Repositories;

public class WorkoutSessionRepositoryMongoDb(
    ILogger<WorkoutSessionRepositoryMongoDb> logger,
    IMongoDatabase mongoDatabase)
    : IWorkSessionRepository
{
    private readonly IMongoCollection<WorkoutSession> _workoutSessionCollection = 
        mongoDatabase.GetCollection<WorkoutSession>("WorkoutSessions");

    public async Task<WorkoutSession> CreateWorkSession(WorkoutSession session)
    {
        logger.LogDebug("CreateSession called from {Repository}", nameof(WorkoutSessionRepositoryMongoDb));

        try
        {
            session.SessionId = await GetMaxId() + 1;
            await _workoutSessionCollection.InsertOneAsync(session);
            return session;
        }
        catch (Exception e)
        {
            logger.LogError(e, "CreateSession failed");
            throw;
        }
    }

    private async Task<int> GetMaxId()
    {
        var sort = Builders<WorkoutSession>.Sort.Descending(s => s.SessionId);
        var result = await _workoutSessionCollection.Find(_ => true).Sort(sort).FirstOrDefaultAsync();
        return result?.SessionId ?? 0;
    }
}