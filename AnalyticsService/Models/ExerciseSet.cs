namespace AnalyticsService.Models;

public class ExerciseSet
{
    public int SetId { get; set; } 
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
}