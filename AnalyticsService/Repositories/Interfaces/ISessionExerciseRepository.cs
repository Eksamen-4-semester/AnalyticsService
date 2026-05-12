using AnalyticsService.Models;

namespace AnalyticsService.Repositories;

public interface ISessionExerciseRepository
{
    Task<SessionExercise?> AddExerciseToSession(int memberId, int sessionId, SessionExercise exercise);
    Task<SessionExercise?> UpdateExerciseInSession(int memberId, int sessionId, int exerciseId, SessionExercise updatedExercise);
    Task<SessionExercise?> DeleteExerciseFromSession(int memberId, int sessionId, int exerciseId);
}
