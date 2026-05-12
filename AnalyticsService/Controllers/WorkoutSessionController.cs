using AnalyticsService.DTOs;
using AnalyticsService.Models;
using AnalyticsService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers;

public class WorkoutSessionController : Controller
{
    
    private readonly IWorkSessionRepository _workOutSessionRepository;
    private readonly ILogger<WorkoutSessionController> _logger;
    
    public WorkoutSessionController(
        ILogger<WorkoutSessionController> logger,
        IWorkSessionRepository workoutSessionRepository)
    {
        _logger = logger;
        _workOutSessionRepository = workoutSessionRepository;
    }

    [HttpPost]
    [Route("/api/members/{memberId}/sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateWorkoutSession(int memberId, [FromBody] CreateSessionDto dto)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId}", nameof(CreateWorkoutSession), memberId);
 
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            _logger.LogWarning("Create session failed — title is missing for memberId: {MemberId}", memberId);
            return BadRequest("Session title is required");
        }
 
        if (dto.DurationMinutes <= 0)
        {
            _logger.LogWarning("Create session failed — invalid duration for memberId: {MemberId}", memberId);
            return BadRequest("Duration must be greater than 0");
        }
 
        var session = new WorkoutSession
        {
            MemberId = memberId,
            Title = dto.Title,
            MuscleGroup = dto.MuscleGroup,
            Date = dto.Date,
            DurationMinutes = dto.DurationMinutes,
            Exercises = dto.Exercises.Select(e => new SessionExercise
            {
                ExerciseName = e.ExerciseName,
                Order = e.Order,
                Sets = e.Sets.Select(s => new ExerciseSet
                {
                    SetNumber = s.SetNumber,
                    Reps = s.Reps,
                    WeightKg = s.WeightKg,
                    DurationSeconds = s.DurationSeconds
                }).ToList()
            }).ToList()
        };

        await _workOutSessionRepository.CreateWorkSession(session);
 
        _logger.LogInformation("Session created successfully. SessionId: {SessionId}", session.SessionId);
        return Created($"/api/members/{memberId}/sessions/{session.SessionId}", session);
    }
}