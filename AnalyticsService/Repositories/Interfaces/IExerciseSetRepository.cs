using AnalyticsService.Models;

namespace AnalyticsService.Repositories;

public interface IExerciseSetRepository
{
    Task<ExerciseSet?> AddSetToExercise(int memberId, int sessionId, int exerciseId, ExerciseSet set);
    Task<ExerciseSet?> UpdateSetInExercise(int memberId, int sessionId, int exerciseId, int setId, ExerciseSet updatedSet);
    Task<ExerciseSet?> DeleteSetFromExercise(int memberId, int sessionId, int exerciseId, int setId);
}