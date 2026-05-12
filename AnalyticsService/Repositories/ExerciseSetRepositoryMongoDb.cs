using AnalyticsService.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AnalyticsService.Repositories;

public class ExerciseSetRepositoryMongoDb : IExerciseSetRepository
{
    private readonly IMongoCollection<WorkoutSession> _workoutSessionCollection;

    public ExerciseSetRepositoryMongoDb(IMongoDatabase mongoDatabase)
    {
        _workoutSessionCollection = mongoDatabase.GetCollection<WorkoutSession>("WorkoutSessions");
    }

    public async Task<ExerciseSet?> AddSetToExercise(int memberId, int sessionId, int exerciseId, ExerciseSet set)
    {
        var filter = Builders<WorkoutSession>.Filter.And(
            Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId),
            Builders<WorkoutSession>.Filter.Eq(x => x.SessionId, sessionId),
            Builders<WorkoutSession>.Filter.ElemMatch(x => x.Exercises, e => e.ExerciseId == exerciseId)
        );

        var update = Builders<WorkoutSession>.Update.Push("Exercises.$.Sets", set);

        var result = await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update);
        return result != null ? set : null;
    }

    public async Task<ExerciseSet?> UpdateSetInExercise(int memberId, int sessionId, int exerciseId, int setId, ExerciseSet updatedSet)
    {
        var filter = Builders<WorkoutSession>.Filter.And(
            Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId),
            Builders<WorkoutSession>.Filter.Eq(x => x.SessionId, sessionId),
            Builders<WorkoutSession>.Filter.ElemMatch(x => x.Exercises, e => e.ExerciseId == exerciseId && e.Sets.Any(s => s.SetId == setId))
        );

        var update = Builders<WorkoutSession>.Update.Set("Exercises.$[exercise].Sets.$[set]", updatedSet);
        
        var arrayFilters = new ArrayFilterDefinition<BsonDocument>[]
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("exercise.ExerciseId", exerciseId)),
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("set.SetId", setId))
        };
        
        var options = new FindOneAndUpdateOptions<WorkoutSession> { ArrayFilters = arrayFilters };

        var result = await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);
        return result != null ? updatedSet : null;
    }

    public async Task<ExerciseSet?> DeleteSetFromExercise(int memberId, int sessionId, int exerciseId, int setId)
    {
        var filter = Builders<WorkoutSession>.Filter.And(
            Builders<WorkoutSession>.Filter.Eq(x => x.MemberId, memberId),
            Builders<WorkoutSession>.Filter.Eq(x => x.SessionId, sessionId)
        );

        var update = Builders<WorkoutSession>.Update.PullFilter(
            "Exercises.$[exercise].Sets",
            Builders<ExerciseSet>.Filter.Eq(s => s.SetId, setId)
        );

        var arrayFilters = new ArrayFilterDefinition<BsonDocument>[]
        {
            new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("exercise.ExerciseId", exerciseId))
        };

        var options = new FindOneAndUpdateOptions<WorkoutSession>
        {
            ArrayFilters = arrayFilters
        };

        var sessionBeforeUpdate = await _workoutSessionCollection.FindOneAndUpdateAsync(filter, update, options);

        if (sessionBeforeUpdate == null)
        {
            return null;
        }

        var exercise = sessionBeforeUpdate.Exercises?.FirstOrDefault(e => e.ExerciseId == exerciseId);
        var setToDelete = exercise?.Sets?.FirstOrDefault(s => s.SetId == setId);

        return setToDelete;
    }
}