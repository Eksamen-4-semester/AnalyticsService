using AnalyticsService.DTOs;
using AnalyticsService.Models;
namespace AnalyticsService.DTOs;

    public class CreateSessionDto
    {
        public string Title { get; set; } = string.Empty;
        public MuscleGroup MuscleGroup { get; set; }
        public DateTime Date { get; set; }
        public int DurationMinutes { get; set; }
        public string? Notes { get; set; }
        public List<CreateExerciseDto> Exercises { get; set; } = new();
    }
    