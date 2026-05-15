using AnalyticsService.DTOs;
using AnalyticsService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace AnalyticsService.Controllers
{
    [ApiController]
    [Route("api/members")]
    public class DashboardController : ControllerBase
    {
        private readonly ILogger<DashboardController> _logger;
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(ILogger<DashboardController> logger, IDashboardRepository dashboardRepository)
        {
            _logger = logger;
            _dashboardRepository = dashboardRepository;
        }

        [HttpGet("{memberId}/dashboard")]
        public async Task<IActionResult> GetDashboard(int memberId)
        {
            var sessions = await _dashboardRepository.GetSessionsByMemberId(memberId);

            if (sessions == null || !sessions.Any())
            {
                return NotFound("No workout sessions found for this member.");
            }

            var totalSessions = sessions.Count;
            var totalDurationMinutes = sessions.Sum(s => s.DurationMinutes);
            var totalWeightLifted = sessions.SelectMany(s => s.Exercises)
                                            .SelectMany(e => e.Sets)
                                            .Sum(set => set.Reps * set.WeightKg);
            var totalReps = sessions.SelectMany(s => s.Exercises)
                                  .SelectMany(e => e.Sets)
                                  .Sum(set => set.Reps);
            var totalSets = sessions.SelectMany(s => s.Exercises)
                                  .SelectMany(e => e.Sets)
                                  .Count();
            var favoriteMuscleGroup = sessions.GroupBy(s => s.MuscleGroup)
                                              .OrderByDescending(g => g.Count())
                                              .FirstOrDefault()?.Key.ToString();
            
            var totalCalories = totalDurationMinutes * 4.5m;

            var dashboardDto = new DashboardDto
            {
                TotalSessions = totalSessions,
                TotalDurationMinutes = totalDurationMinutes,
                TotalWeightLifted = totalWeightLifted,
                TotalReps = totalReps,
                TotalSets = totalSets,
                EstimatedCalories = totalCalories,  
                FavoriteMuscleGroup = favoriteMuscleGroup
            };

            return Ok(dashboardDto);
        }
    }
}
