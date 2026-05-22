using AnalyticsService.Models;
using MongoDB.Driver;

namespace AnalyticsService.Repositories;

public class SessionRepositoryMongoDb : ISessionExerciseRepository
{
    private readonly IMongoCollection<WorkoutSession> _workoutSessionCollection;

    public SessionRepositoryMongoDb(IMongoDatabase mongoDatabase)
    {
        _workoutSessionCollection = mongoDatabase.GetCollection<WorkoutSession>("WorkoutSessions");
    }

    public async Task<SessionExercise?> AddExerciseToSession(int memberId, int sessionId, SessionExercise exercise)
    {
        var filter = Builders<WorkoutSession>.Filter.Where(s => s.MemberId == memberId && s.SessionId == sessionId);
        var session = await _workoutSessionCollection.Find(filter).FirstOrDefaultAsync();
        if (session == null) return null;
        
        exercise.ExerciseId = session.Exercises.Any()
            ? session.Exercises.Max(e => e.ExerciseId) + 1
            : 1;

        var update = Builders<WorkoutSession>.Update.Push(s => s.Exercises, exercise);
        var options = new FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>
        {
            ReturnDocument = ReturnDocument.After
        };
        var updatedSession = await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);
        return updatedSession?.Exercises.LastOrDefault();
    }

    public async Task<SessionExercise?> UpdateExerciseInSession(int memberId, int sessionId, int exerciseId, SessionExercise updatedExercise)
    {
        var filter = Builders<WorkoutSession>.Filter.Where(s => s.MemberId == memberId && s.SessionId == sessionId && s.Exercises.Any(e => e.ExerciseId == exerciseId));
        var update = Builders<WorkoutSession>.Update.Set("Exercises.$", updatedExercise);
        var options = new FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>
        {
            ReturnDocument = ReturnDocument.After
        };
        var updatedSession = await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);
        return updatedSession?.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
    }

    public async Task<SessionExercise?> DeleteExerciseFromSession(int memberId, int sessionId, int exerciseId)
    {
        var filter = Builders<WorkoutSession>.Filter.Where(s => s.MemberId == memberId && s.SessionId == sessionId);
        var update = Builders<WorkoutSession>.Update.PullFilter(s => s.Exercises, e => e.ExerciseId == exerciseId);
        var session = await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update);
        return session?.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
    }
}