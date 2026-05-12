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
    public async Task<WorkoutSession?> DeleteWorkSession(int memberId, int sessionId)
    {
        var filter = Builders<WorkoutSession>.Filter.And(
            Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId),
            Builders<WorkoutSession>.Filter.Eq(x => x.SessionId, sessionId)
        );

        return await _workoutSessionCollection.FindOneAndDeleteAsync(filter);
    }
    public async Task<WorkoutSession?> DeleteExerciseFromSession(int memberId, int sessionId, int exerciseId)
    {
        var filter = Builders<WorkoutSession>.Filter.And(
            Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId),
            Builders<WorkoutSession>.Filter.Eq(x => x.SessionId, sessionId)
        );

        var update = Builders<WorkoutSession>.Update.PullFilter(
            x => x.Exercises,
            Builders<SessionExercise>.Filter.Eq(e => e.ExerciseId, exerciseId)
        );

        var options = new FindOneAndUpdateOptions<WorkoutSession>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);
    }
    
    public async Task<WorkoutSession?> DeleteSetFromExercise(int memberId, int sessionId, int exerciseId, int setId)
    {
        var filter = Builders<WorkoutSession>.Filter.And(
            Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId),
            Builders<WorkoutSession>.Filter.Eq(x => x.SessionId, sessionId),
            Builders<WorkoutSession>.Filter.ElemMatch(x => x.Exercises, 
                e => e.ExerciseId == exerciseId)
        );

        var update = Builders<WorkoutSession>.Update.PullFilter(
            "Exercises.$.Sets",
            Builders<ExerciseSet>.Filter.Eq(s => s.SetId, setId)
        );

        var options = new FindOneAndUpdateOptions<WorkoutSession>
        {
            ReturnDocument = ReturnDocument.After
        };

        return await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);
    }

    private async Task<int> GetMaxId()
    {
        var sort = Builders<WorkoutSession>.Sort.Descending(s => s.SessionId);
        var result = await _workoutSessionCollection.Find(_ => true).Sort(sort).FirstOrDefaultAsync();
        return result?.SessionId ?? 0;
    }
}