using AnalyticsService.Controllers;
using AnalyticsService.DTOs;
using AnalyticsService.Models;
using AnalyticsService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
namespace AnalyticsService.Tests;

[TestClass]
public class WorkoutSessionControllerTests
{
    private Mock<IWorkSessionRepository> _repoMock = new();
    private Mock<ILogger<WorkoutSessionController>> _loggerMock = new();
    private WorkoutSessionController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _repoMock = new Mock<IWorkSessionRepository>();
        _loggerMock = new Mock<ILogger<WorkoutSessionController>>();
        _sut = new WorkoutSessionController(_loggerMock.Object, _repoMock.Object);
    }

    // ── CreateWorkoutSession ──────────────────────────────────────────

    [TestMethod]
    public async Task CreateWorkoutSession_WhenValid_Returns201()
    {
        // Arrange
        var dto = new CreateSessionDto
        {
            Title = "Bryst dag",
            MuscleGroup = MuscleGroup.Chest,
            Date = DateTime.UtcNow,
            DurationMinutes = 60,
            Exercises = []
        };

        var createdSession = new WorkoutSession { SessionId = 1, MemberId = 42, Title = dto.Title };
        _repoMock.Setup(r => r.CreateWorkSession(It.IsAny<WorkoutSession>())).ReturnsAsync(createdSession);

        // Act
        var result = await _sut.CreateWorkoutSession(42, dto);

        // Assert
        var created = result as CreatedResult;
        Assert.IsNotNull(created);
        Assert.AreEqual(201, created.StatusCode);
    }

    [TestMethod]
    public async Task CreateWorkoutSession_WhenTitleMissing_Returns400()
    {
        // Arrange
        var dto = new CreateSessionDto
        {
            Title = "",
            DurationMinutes = 60,
            Exercises = []
        };

        // Act
        var result = await _sut.CreateWorkoutSession(42, dto);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual("Session title is required", badRequest.Value);
        _repoMock.Verify(r => r.CreateWorkSession(It.IsAny<WorkoutSession>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateWorkoutSession_WhenDurationIsZero_Returns400()
    {
        // Arrange
        var dto = new CreateSessionDto
        {
            Title = "Ryg dag",
            DurationMinutes = 0,
            Exercises = []
        };

        // Act
        var result = await _sut.CreateWorkoutSession(42, dto);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual("Duration must be greater than 0", badRequest.Value);
        _repoMock.Verify(r => r.CreateWorkSession(It.IsAny<WorkoutSession>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateWorkoutSession_MapsMemberIdFromRoute()
    {
        // Arrange
        var dto = new CreateSessionDto
        {
            Title = "Ben dag",
            DurationMinutes = 45,
            Exercises = []
        };

        WorkoutSession? capturedSession = null;
        _repoMock.Setup(r => r.CreateWorkSession(It.IsAny<WorkoutSession>()))
            .Callback<WorkoutSession>(s => capturedSession = s)
            .ReturnsAsync(new WorkoutSession { SessionId = 1 });

        // Act
        await _sut.CreateWorkoutSession(99, dto);

        // Assert
        Assert.IsNotNull(capturedSession);
        Assert.AreEqual(99, capturedSession.MemberId);
    }

    // ── DeleteWorkoutSession ──────────────────────────────────────────

    [TestMethod]
    public async Task DeleteWorkoutSession_WhenExists_Returns204()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteWorkSession(1, 1))
            .ReturnsAsync(new WorkoutSession { SessionId = 1 });

        // Act
        var result = await _sut.DeleteWorkoutSession(1, 1);

        // Assert
        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    [TestMethod]
    public async Task DeleteWorkoutSession_WhenNotFound_Returns404()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteWorkSession(1, 99))
            .ReturnsAsync((WorkoutSession?)null);

        // Act
        var result = await _sut.DeleteWorkoutSession(1, 99);

        // Assert
        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    // ── DeleteExerciseFromSession ─────────────────────────────────────

    [TestMethod]
    public async Task DeleteExerciseFromSession_WhenExists_Returns204()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteExerciseFromSession(1, 1, 1))
            .ReturnsAsync(new WorkoutSession { SessionId = 1 });

        // Act
        var result = await _sut.DeleteExerciseFromSession(1, 1, 1);

        // Assert
        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    [TestMethod]
    public async Task DeleteExerciseFromSession_WhenNotFound_Returns404()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteExerciseFromSession(1, 1, 99))
            .ReturnsAsync((WorkoutSession?)null);

        // Act
        var result = await _sut.DeleteExerciseFromSession(1, 1, 99);

        // Assert
        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    // ── DeleteSetFromExercise ─────────────────────────────────────────

    [TestMethod]
    public async Task DeleteSetFromExercise_WhenExists_Returns204()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteSetFromExercise(1, 1, 1, 1))
            .ReturnsAsync(new WorkoutSession { SessionId = 1 });

        // Act
        var result = await _sut.DeleteSetFromExercise(1, 1, 1, 1);

        // Assert
        Assert.IsInstanceOfType<NoContentResult>(result);
    }

    [TestMethod]
    public async Task DeleteSetFromExercise_WhenNotFound_Returns404()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteSetFromExercise(1, 1, 1, 99))
            .ReturnsAsync((WorkoutSession?)null);

        // Act
        var result = await _sut.DeleteSetFromExercise(1, 1, 1, 99);

        // Assert
        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }
}