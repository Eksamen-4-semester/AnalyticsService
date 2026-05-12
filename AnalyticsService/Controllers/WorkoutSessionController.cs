using AnalyticsService.DTOs;
using AnalyticsService.Models;
using AnalyticsService.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers;

public class WorkoutSessionController : Controller
{
    
    private readonly IWorkoutSessionRepository _workoutSessionRepository;
    private readonly ILogger<WorkoutSessionController> _logger;
    
    public WorkoutSessionController(
        ILogger<WorkoutSessionController> logger,
        IWorkoutSessionRepository workoutSessionRepository)
    {
        _logger = logger;
        _workoutSessionRepository = workoutSessionRepository;
    }
    // Create a new workout session for a member
    [HttpPost]
    [Route("/api/members/{memberId}/sessions")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
 
        await _workoutSessionRepository.CreateWorkSession(session);
        _logger.LogInformation("Session created successfully. SessionId: {SessionId}", session.SessionId);
        return Created($"/api/members/{memberId}/sessions/{session.SessionId}", session);
    }
    // Get all workout sessions for a member
    [HttpGet]
    [Route("/api/members/{memberId}/sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllWorkoutSessions(int memberId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId}", nameof(GetAllWorkoutSessions), memberId);
 
        var sessions = await _workoutSessionRepository.GetAllWorkSessionsByMemberId(memberId);
        if (sessions.Count == 0)
        {
            _logger.LogWarning("No sessions found for memberId: {MemberId}", memberId);
            return NotFound($"No sessions found for member {memberId}");
        }
 
        _logger.LogInformation("Returning {Count} sessions for memberId: {MemberId}", sessions.Count, memberId);
        return Ok(sessions);
    }
    // Get a specific workout session for a member
    [HttpGet]
    [Route("/api/members/{memberId}/sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWorkoutSessionById(int memberId, int sessionId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId}", nameof(GetWorkoutSessionById), memberId, sessionId);
 
        var session = await _workoutSessionRepository.GetWorkSessionById(memberId, sessionId);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found for memberId: {MemberId}", sessionId, memberId);
            return NotFound($"Session with id {sessionId} not found");
        }
 
        return Ok(session);
    }
    
    // Update an existing workout session for a member
    [HttpPut]
    [Route("/api/members/{memberId}/sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateWorkoutSession(int memberId, int sessionId, [FromBody] CreateSessionDto dto)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId}", nameof(UpdateWorkoutSession), memberId, sessionId);
 
        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            _logger.LogWarning("Update session failed — title is missing");
            return BadRequest("Session title is required");
        }
 
        var updatedSession = new WorkoutSession
        {
            MemberId = memberId,
            SessionId = sessionId,
            Title = dto.Title,
            MuscleGroup = dto.MuscleGroup,
            Date = dto.Date,
            DurationMinutes = dto.DurationMinutes,
        };
 
        var result = await _workoutSessionRepository.UpdateWorkSession(memberId, sessionId, updatedSession);
        if (result == null)
        {
            _logger.LogWarning("Update session failed — sessionId: {SessionId} not found", sessionId);
            return NotFound($"Session with id {sessionId} not found");
        }
 
        _logger.LogInformation("Session updated successfully. SessionId: {SessionId}", sessionId);
        return Ok(result);
    }
    
    // Delete a workout session for a member
    [HttpDelete]
    [Route("/api/members/{memberId}/sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWorkoutSession(int memberId, int sessionId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId}", nameof(DeleteWorkoutSession), memberId, sessionId);
 
        var deleted = await _workoutSessionRepository.DeleteWorkSession(memberId, sessionId);
        if (deleted == null)
        {
            _logger.LogWarning("Delete session failed — sessionId: {SessionId} not found for memberId: {MemberId}", sessionId, memberId);
            return NotFound($"Session with id {sessionId} not found");
        }
 
        _logger.LogInformation("Session deleted successfully. SessionId: {SessionId}", sessionId);
        return NoContent();
    }
}