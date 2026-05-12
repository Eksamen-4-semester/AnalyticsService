using AnalyticsService.Models;
using AnalyticsService.Repositories.Interfaces;
using MongoDB.Driver;

namespace AnalyticsService.Repositories;

public class WorkoutSessionRepositoryMongoDb(
    ILogger<WorkoutSessionRepositoryMongoDb> logger,
    IMongoDatabase mongoDatabase)
    : IWorkoutSessionRepository
{
    private readonly IMongoCollection<WorkoutSession> _workoutSessionCollection = 
        mongoDatabase.GetCollection<WorkoutSession>("WorkoutSessions");

    public async Task<WorkoutSession> CreateWorkSession(WorkoutSession session)
    {
        session.SessionId = await GetMaxId() + 1;
        session.StartedAt = DateTime.UtcNow;
        await _workoutSessionCollection.InsertOneAsync(session);
        return session;
    }

    public async Task<WorkoutSession?> GetWorkSessionById(int memberId, int sessionId)
    {
        var cursor = await _workoutSessionCollection.FindAsync(s => s.MemberId == memberId && s.SessionId == sessionId);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<List<WorkoutSession>> GetAllWorkSessionsByMemberId(int memberId)
    {
        var cursor = await _workoutSessionCollection.FindAsync(s => s.MemberId == memberId);
        return await cursor.ToListAsync();
    }

    public async Task<WorkoutSession?> UpdateWorkSession(int memberId, int sessionId, WorkoutSession updatedSession)
    {
        var filter = Builders<WorkoutSession>.Filter.Where(s => s.MemberId == memberId && s.SessionId == sessionId);
        var options = new FindOneAndReplaceOptions<WorkoutSession, WorkoutSession>
        {
            ReturnDocument = ReturnDocument.After
        };
        return await _workoutSessionCollection.FindOneAndReplaceAsync(filter, updatedSession, options);
    }
    
    public async Task<WorkoutSession?> DeleteWorkSession(int memberId, int sessionId)
    {
        var filter = Builders<WorkoutSession>.Filter.Where(s => s.MemberId == memberId && s.SessionId == sessionId);
        return await _workoutSessionCollection.FindOneAndDeleteAsync(filter);
    }

    public async Task<WorkoutSession?> CompleteWorkoutSession(int memberId, int sessionId)
    {
        var filter = Builders<WorkoutSession>.Filter.Where(s => s.MemberId == memberId && s.SessionId == sessionId);
        var update = Builders<WorkoutSession>.Update.Set(s => s.EndedAt, DateTime.UtcNow);
        var options = new FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>
        {
            ReturnDocument = ReturnDocument.After
        };
        return await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);
    }

    private async Task<int> GetMaxId()
    {
        var sort = Builders<WorkoutSession>.Sort.Descending(s => s.SessionId);
        var options = new FindOptions<WorkoutSession> { Sort = sort, Limit = 1 };
        var cursor = await _workoutSessionCollection.FindAsync(_ => true, options);
        var result = await cursor.FirstOrDefaultAsync();
        return result?.SessionId ?? 0;
    }
}