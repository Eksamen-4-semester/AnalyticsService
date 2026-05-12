using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using AnalyticsService.Models;
using Microsoft.AspNetCore.Mvc;
using AnalyticsService.DTOs;
using AnalyticsService.Repositories.Interfaces;
using AnalyticsService.Controllers;

namespace AnalyticsService.Tests;

public class DashboardControllerTests
{
    private readonly Mock<IDashboardRepository> _mockRepo;
    private readonly DashboardController _controller;

    public DashboardControllerTests()
    {
        _mockRepo = new Mock<IDashboardRepository>();
        var mockLogger = new Mock<ILogger<DashboardController>>();
        _controller = new DashboardController(mockLogger.Object, _mockRepo.Object);
    }

    [Fact]
    public async Task GetDashboard_ReturnsNotFound_WhenNoSessionsExist()
    {
        // Arrange
        var memberId = 1;
        _mockRepo.Setup(repo => repo.GetSessionsByMemberId(memberId))
            .ReturnsAsync(new List<WorkoutSession>());

        // Act
        var result = await _controller.GetDashboard(memberId);

        // Assert
        Xunit.Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetDashboard_ReturnsDashboardDto_WhenSingleSessionExists()
    {
        // Arrange
        var memberId = 1;
        var sessions = new List<WorkoutSession>
        {
            new WorkoutSession
            {
                Id = "1",
                MemberId = memberId,
                Date = DateTime.UtcNow.AddDays(-1),
                DurationMinutes = 30,
                MuscleGroup = MuscleGroup.Legs,
                Exercises = new List<SessionExercise>
                {
                    new SessionExercise
                    {
                        Sets = new List<ExerciseSet>
                        {
                            new ExerciseSet { Reps = 12, WeightKg = 120 },
                            new ExerciseSet { Reps = 10, WeightKg = 130 }
                        }
                    }
                }
            }
        };
        _mockRepo.Setup(repo => repo.GetSessionsByMemberId(memberId))
            .ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetDashboard(memberId);

        // Assert
        var okResult = Xunit.Assert.IsType<OkObjectResult>(result);
        var dashboard = Xunit.Assert.IsType<DashboardDto>(okResult.Value);

        Xunit.Assert.Equal(1, dashboard.TotalSessions);
        Xunit.Assert.Equal(30, dashboard.TotalDurationMinutes);
        Xunit.Assert.Equal((12 * 120) + (10 * 130), dashboard.TotalWeightLifted);
        Xunit.Assert.Equal(12 + 10, dashboard.TotalReps);
        Xunit.Assert.Equal(2, dashboard.TotalSets);
        Xunit.Assert.Equal("Legs", dashboard.FavoriteMuscleGroup);
        Xunit.Assert.Equal(30 * 4.5m, dashboard.EstimatedCalories);
    }

    [Fact]
    public async Task GetDashboard_ReturnsDashboardDto_WhenSessionsExist()
    {
        // Arrange
        var memberId = 1;
        var sessions = new List<WorkoutSession>
        {
            new WorkoutSession
            {
                Id = "1",
                MemberId = memberId,
                Date = DateTime.UtcNow.AddDays(-1),
                DurationMinutes = 60,
                MuscleGroup = MuscleGroup.Chest,
                Exercises = new List<SessionExercise>
                {
                    new SessionExercise
                    {
                        Sets = new List<ExerciseSet>
                        {
                            new ExerciseSet { Reps = 10, WeightKg = 100 },
                            new ExerciseSet { Reps = 8, WeightKg = 110 }
                        }
                    }
                }
            },
            new WorkoutSession
            {
                Id = "2",
                MemberId = memberId,
                Date = DateTime.UtcNow.AddDays(-2),
                DurationMinutes = 45,
                MuscleGroup = MuscleGroup.Back,
                Exercises = new List<SessionExercise>
                {
                    new SessionExercise
                    {
                        Sets = new List<ExerciseSet>
                        {
                            new ExerciseSet { Reps = 12, WeightKg = 80 },
                            new ExerciseSet { Reps = 10, WeightKg = 85 }
                        }
                    }
                }
            }
        };
        _mockRepo.Setup(repo => repo.GetSessionsByMemberId(memberId))
            .ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetDashboard(memberId);

        // Assert
        var okResult = Xunit.Assert.IsType<OkObjectResult>(result);
        var dashboard = Xunit.Assert.IsType<DashboardDto>(okResult.Value);

        Xunit.Assert.Equal(2, dashboard.TotalSessions);
        Xunit.Assert.Equal(105, dashboard.TotalDurationMinutes);
        Xunit.Assert.Equal((10 * 100) + (8 * 110) + (12 * 80) + (10 * 85), dashboard.TotalWeightLifted);
        Xunit.Assert.Equal(10 + 8 + 12 + 10, dashboard.TotalReps);
        Xunit.Assert.Equal(4, dashboard.TotalSets);
        Xunit.Assert.Equal("Chest", dashboard.FavoriteMuscleGroup);
    }
}
