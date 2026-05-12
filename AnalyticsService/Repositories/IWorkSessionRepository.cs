using AnalyticsService.Models;
namespace AnalyticsService.Repositories;

public interface IWorkSessionRepository 
{
    Task<WorkoutSession> CreateWorkSession(WorkoutSession session);
    Task<WorkoutSession?> DeleteWorkSession(int memberId, int sessionId);
    Task<WorkoutSession?> DeleteExerciseFromSession(int memberId, int sessionId, int exerciseId);
    Task<WorkoutSession?> DeleteSetFromExercise(int memberId, int sessionId, int exerciseId, int setId);
}