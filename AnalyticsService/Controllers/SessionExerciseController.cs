using AnalyticsService.DTOs;
using AnalyticsService.Models;
using AnalyticsService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers;

public class SessionExerciseController: Controller
{
    private readonly ISessionExerciseRepository _sessionExerciseRepository;
    private readonly ILogger<SessionExerciseController> _logger;
    
    public SessionExerciseController(
        ILogger<SessionExerciseController> logger,
        ISessionExerciseRepository sessionExerciseRepository)
    {
        _logger = logger;
        _sessionExerciseRepository = sessionExerciseRepository;
    }
    
     [HttpPost]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddExerciseToSession(int memberId, int sessionId, [FromBody] CreateExerciseDto dto)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId}", nameof(AddExerciseToSession), memberId, sessionId);
 
        if (string.IsNullOrWhiteSpace(dto.ExerciseName))
        {
            _logger.LogWarning("Add exercise failed — exercise name is missing");
            return BadRequest("Exercise name is required");
        }
 
        var exercise = new SessionExercise
        {
            ExerciseName = dto.ExerciseName,
            Order = dto.Order,
            Sets = []
        };
 
        var result = await _sessionExerciseRepository.AddExerciseToSession(memberId, sessionId, exercise);
        if (result == null)
        {
            _logger.LogWarning("Add exercise failed — sessionId: {SessionId} not found", sessionId);
            return NotFound($"Session with id {sessionId} not found");
        }
 
        _logger.LogInformation("Exercise added successfully to sessionId: {SessionId}", sessionId);
        return Created($"/api/members/{memberId}/sessions/{sessionId}", result);
    }
 
    [HttpPut]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExerciseInSession(int memberId, int sessionId, int exerciseId, [FromBody] CreateExerciseDto dto)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId}", nameof(UpdateExerciseInSession), memberId, sessionId, exerciseId);
 
        var updatedExercise = new SessionExercise
        {
            ExerciseId = exerciseId,
            ExerciseName = dto.ExerciseName,
            Order = dto.Order
        };
 
        var result = await _sessionExerciseRepository.UpdateExerciseInSession(memberId, sessionId, exerciseId, updatedExercise);
        if (result == null)
        {
            _logger.LogWarning("Update exercise failed — exerciseId: {ExerciseId} not found", exerciseId);
            return NotFound($"Exercise with id {exerciseId} not found");
        }
 
        _logger.LogInformation("Exercise updated successfully. ExerciseId: {ExerciseId}", exerciseId);
        return Ok(result);
    }
 
    [HttpDelete]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExerciseFromSession(int memberId, int sessionId, int exerciseId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId}", nameof(DeleteExerciseFromSession), memberId, sessionId, exerciseId);
 
        var deleted = await _sessionExerciseRepository.DeleteExerciseFromSession(memberId, sessionId, exerciseId);
        if (deleted == null)
        {
            _logger.LogWarning("Delete exercise failed — exerciseId: {ExerciseId} not found", exerciseId);
            return NotFound($"Exercise with id {exerciseId} not found");
        }
 
        _logger.LogInformation("Exercise deleted successfully. ExerciseId: {ExerciseId}", exerciseId);
        return NoContent();
    }
}