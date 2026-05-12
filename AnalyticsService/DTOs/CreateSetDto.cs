namespace AnalyticsService.DTOs;

public class CreateSetDto
{
    public int SetNumber { get; set; }
    public int Reps { get; set; }
    public decimal WeightKg { get; set; }
    public int? DurationSeconds { get; set; }
}