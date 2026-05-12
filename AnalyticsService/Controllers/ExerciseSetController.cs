using AnalyticsService.DTOs;
using AnalyticsService.Models;
using AnalyticsService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticsService.Controllers;

[ApiController]
public class ExerciseSetController : ControllerBase
{
    private readonly IExerciseSetRepository _setRepository;
    private readonly ILogger<ExerciseSetController> _logger;
    
    public ExerciseSetController(
        ILogger<ExerciseSetController> logger,
        IExerciseSetRepository setRepository)
    {
        _logger = logger;
        _setRepository = setRepository;
    }
    
   [HttpPost]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}/sets")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddSetToExercise(int memberId, int sessionId, int exerciseId, [FromBody] CreateSetDto dto)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId}", nameof(AddSetToExercise), memberId, sessionId, exerciseId);
 
        var set = new ExerciseSet
        {
            SetNumber = dto.SetNumber,
            Reps = dto.Reps,
            WeightKg = dto.WeightKg,
            DurationSeconds = dto.DurationSeconds
        };
 
        var result = await _setRepository.AddSetToExercise(memberId, sessionId, exerciseId, set);
        if (result == null)
        {
            _logger.LogWarning("Add set failed — exerciseId: {ExerciseId} not found", exerciseId);
            return NotFound($"Exercise with id {exerciseId} not found");
        }
 
        _logger.LogInformation("Set added successfully to exerciseId: {ExerciseId}", exerciseId);
        return Created($"/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}", result);
    }
 
    [HttpPut]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}/sets/{setId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSetInExercise(int memberId, int sessionId, int exerciseId, int setId, [FromBody] CreateSetDto dto)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId} setId: {SetId}", nameof(UpdateSetInExercise), memberId, sessionId, exerciseId, setId);
 
        var updatedSet = new ExerciseSet
        {
            SetId = setId,
            SetNumber = dto.SetNumber,
            Reps = dto.Reps,
            WeightKg = dto.WeightKg,
            DurationSeconds = dto.DurationSeconds
        };
 
        var result = await _setRepository.UpdateSetInExercise(memberId, sessionId, exerciseId, setId, updatedSet);
        if (result == null)
        {
            _logger.LogWarning("Update set failed — setId: {SetId} not found", setId);
            return NotFound($"Set with id {setId} not found");
        }
 
        _logger.LogInformation("Set updated successfully. SetId: {SetId}", setId);
        return Ok(result);
    }
 
    [HttpDelete]
    [Route("/api/members/{memberId}/sessions/{sessionId}/exercises/{exerciseId}/sets/{setId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSetFromExercise(int memberId, int sessionId, int exerciseId, int setId)
    {
        _logger.LogInformation("Called {Function} for memberId: {MemberId} sessionId: {SessionId} exerciseId: {ExerciseId} setId: {SetId}", nameof(DeleteSetFromExercise), memberId, sessionId, exerciseId, setId);
 
        var deleted = await _setRepository.DeleteSetFromExercise(memberId, sessionId, exerciseId, setId);
        if (deleted == null)
        {
            _logger.LogWarning("Delete set failed — setId: {SetId} not found", setId);
            return NotFound($"Set with id {setId} not found");
        }
 
        _logger.LogInformation("Set deleted successfully. SetId: {SetId}", setId);
        return NoContent();
    }
}