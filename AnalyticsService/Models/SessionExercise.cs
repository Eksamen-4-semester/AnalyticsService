namespace AnalyticsService.Models;

public class SessionExercise
{
    public int ExerciseId { get; set; } 
    public string ExerciseName { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<ExerciseSet> Sets { get; set; } = new();
}