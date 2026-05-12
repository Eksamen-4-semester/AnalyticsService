using AnalyticsService.Models;

namespace AnalyticsService.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<List<WorkoutSession>> GetSessionsByMemberId(int memberId);
}
