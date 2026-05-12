using AnalyticsService.Models;
namespace AnalyticsService.Repositories;

public interface IWorkSessionRepository 
{
    Task<WorkoutSession> CreateWorkSession(WorkoutSession session);
}