using AnalyticsService.Models;
using AnalyticsService.Repositories;
using MongoDB.Driver;
using Moq;

namespace AnalyticsService.Tests;

[TestClass]
public class SessionRepositoryMongoDbTests
{
    private Mock<IMongoDatabase> _mongoDatabaseMock = null!;
    private Mock<IMongoCollection<WorkoutSession>> _workoutSessionCollectionMock = null!;
    private SessionRepositoryMongoDb _sessionRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _workoutSessionCollectionMock = new Mock<IMongoCollection<WorkoutSession>>();
        _mongoDatabaseMock.Setup(db => db.GetCollection<WorkoutSession>("WorkoutSessions", null))
            .Returns(_workoutSessionCollectionMock.Object);
        _sessionRepository = new SessionRepositoryMongoDb(_mongoDatabaseMock.Object);
    }

    [TestMethod]
    public async Task AddExerciseToSession_ShouldReturnExercise_WhenUpdateIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var exercise = new SessionExercise { ExerciseId = 1, ExerciseName = "Test" };
        var updatedSession = new WorkoutSession { MemberId = memberId, SessionId = sessionId, Exercises = new List<SessionExercise> { exercise } };
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndUpdateAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<UpdateDefinition<WorkoutSession>>(), It.IsAny<FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _sessionRepository.AddExerciseToSession(memberId, sessionId, exercise);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(exercise.ExerciseId, result.ExerciseId);
    }

    [TestMethod]
    public async Task UpdateExerciseInSession_ShouldReturnUpdatedExercise_WhenUpdateIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var exerciseId = 1;
        var updatedExercise = new SessionExercise { ExerciseId = 1, ExerciseName = "Updated Test" };
        var updatedSession = new WorkoutSession { MemberId = memberId, SessionId = sessionId, Exercises = new List<SessionExercise> { updatedExercise } };
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndUpdateAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<UpdateDefinition<WorkoutSession>>(), It.IsAny<FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _sessionRepository.UpdateExerciseInSession(memberId, sessionId, exerciseId, updatedExercise);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(updatedExercise.ExerciseName, result.ExerciseName);
    }

    [TestMethod]
    public async Task DeleteExerciseFromSession_ShouldReturnDeletedExercise_WhenDeleteIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var exerciseId = 1;
        var session = new WorkoutSession { MemberId = memberId, SessionId = sessionId, Exercises = new List<SessionExercise> { new SessionExercise { ExerciseId = exerciseId } } };
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndUpdateAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<UpdateDefinition<WorkoutSession>>(), It.IsAny<FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sessionRepository.DeleteExerciseFromSession(memberId, sessionId, exerciseId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(exerciseId, result.ExerciseId);
    }
}