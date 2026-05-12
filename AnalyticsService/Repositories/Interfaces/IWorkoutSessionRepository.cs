using AnalyticsService.Models;
namespace AnalyticsService.Repositories.Interfaces;

public interface IWorkoutSessionRepository 
{
    Task<WorkoutSession> CreateWorkSession(WorkoutSession session);
    Task<WorkoutSession?> GetWorkSessionById(int memberId, int sessionId);
    Task<List<WorkoutSession>> GetAllWorkSessionsByMemberId(int memberId);
    Task<WorkoutSession?> UpdateWorkSession(int memberId, int sessionId, WorkoutSession updatedSession);
    Task<WorkoutSession?> DeleteWorkSession(int memberId, int sessionId);
    Task<WorkoutSession?> CompleteWorkoutSession(int memberId, int sessionId);

}