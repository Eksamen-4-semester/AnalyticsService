namespace AnalyticsService.DTOs;

public class DashboardDto
{
    public int TotalSessions { get; set; }          
    public int TotalDurationMinutes { get; set; }   
    public decimal TotalWeightLifted { get; set; }  
    public int TotalReps { get; set; }             
    public int TotalSets { get; set; }            
    public string FavoriteMuscleGroup { get; set; } = string.Empty; // Mest trænede muskelgruppe
    
    public decimal EstimatedCalories { get; set; }  
    public List<MuscleGroupStatDto> MuscleGroupBreakdown { get; set; } = new();
    public List<WeeklySessionDto> Last7Days { get; set; } = new();
}

public class MuscleGroupStatDto
{
    public string MuscleGroup { get; set; } = string.Empty;
    public int Sessions { get; set; }
}

public class WeeklySessionDto
{
    public string Day { get; set; } = string.Empty;      // "Man", "Tir" osv.
    public int Sessions { get; set; }
    public int DurationMinutes { get; set; }
}