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
    
    [HttpDelete]
    [Route("/api/members/{memberId}/sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkoutSession(int memberId, int sessionId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId}", 
            nameof(DeleteWorkoutSession), memberId, sessionId);

        var deleted = await _workOutSessionRepository.DeleteWorkSession(memberId, sessionId);
        if (deleted == null)
        {
            _logger.LogWarning("Delete session failed — sessionId: {SessionId} not found for memberId: {MemberId}", 
                sessionId, memberId);
            return NotFound($"Session with id {sessionId} not found");
        }

        _logger.LogInformation("Session deleted successfully. SessionId: {SessionId}", sessionId);
        return NoContent();
    }
    
    [HttpDelete]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExerciseFromSession(int memberId, int sessionId, int exerciseId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId}",
            nameof(DeleteExerciseFromSession), memberId, sessionId, exerciseId);

        var deleted = await _workOutSessionRepository.DeleteExerciseFromSession(memberId, sessionId, exerciseId);
        if (deleted == null)
        {
            _logger.LogWarning("Delete exercise failed — exerciseId: {ExerciseId} not found in sessionId: {SessionId}",
                exerciseId, sessionId);
            return NotFound($"Exercise with id {exerciseId} not found in session {sessionId}");
        }

        _logger.LogInformation("Exercise deleted successfully. ExerciseId: {ExerciseId}", exerciseId);
        return NoContent();
    }
    [HttpDelete]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}/sets/{setId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSetFromExercise(int memberId, int sessionId, int exerciseId, int setId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId} setId: {SetId}",
            nameof(DeleteSetFromExercise), memberId, sessionId, exerciseId, setId);

        var deleted = await _workOutSessionRepository.DeleteSetFromExercise(memberId, sessionId, exerciseId, setId);
        if (deleted == null)
        {
            _logger.LogWarning("Delete set failed — setId: {SetId} not found in exerciseId: {ExerciseId}",
                setId, exerciseId);
            return NotFound($"Set with id {setId} not found in exercise {exerciseId}");
        }

        _logger.LogInformation("Set deleted successfully. SetId: {SetId}", setId);
        return NoContent();
    }
}