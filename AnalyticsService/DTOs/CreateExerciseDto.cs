namespace AnalyticsService.DTOs;

public class CreateExerciseDto
{
    public string ExerciseName { get; set; } = string.Empty;
    public int Order { get; set; }
    public List<CreateSetDto> Sets { get; set; } = new();
}