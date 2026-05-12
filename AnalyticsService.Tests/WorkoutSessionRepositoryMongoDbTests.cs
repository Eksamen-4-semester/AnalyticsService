using AnalyticsService.Models;
using AnalyticsService.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Moq;

namespace AnalyticsService.Tests;

[TestClass]
public class WorkoutSessionRepositoryMongoDbTests
{
    private Mock<IMongoDatabase> _mongoDatabaseMock = null!;
    private Mock<IMongoCollection<WorkoutSession>> _workoutSessionCollectionMock = null!;
    private Mock<ILogger<WorkoutSessionRepositoryMongoDb>> _loggerMock = null!;
    private WorkoutSessionRepositoryMongoDb _workoutSessionRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _workoutSessionCollectionMock = new Mock<IMongoCollection<WorkoutSession>>();
        _loggerMock = new Mock<ILogger<WorkoutSessionRepositoryMongoDb>>();
        _mongoDatabaseMock.Setup(db => db.GetCollection<WorkoutSession>("WorkoutSessions", null))
            .Returns(_workoutSessionCollectionMock.Object);
        _workoutSessionRepository = new WorkoutSessionRepositoryMongoDb(_loggerMock.Object, _mongoDatabaseMock.Object);
    }

    [TestMethod]
    public async Task CreateWorkSession_ShouldReturnSession_WhenInsertIsSuccessful()
    {
        // Arrange
        var session = new WorkoutSession { MemberId = 1, Title = "Test" };
        var cursorMock = new Mock<IAsyncCursor<WorkoutSession>>();
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(new[] { new WorkoutSession { SessionId = 1 } });
        _workoutSessionCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<FindOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);
        _workoutSessionCollectionMock.Setup(c => c.InsertOneAsync(session, null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workoutSessionRepository.CreateWorkSession(session);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(session.Title, result.Title);
    }

    [TestMethod]
    public async Task GetWorkSessionById_ShouldReturnSession_WhenSessionExists()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var session = new WorkoutSession { MemberId = memberId, SessionId = sessionId };
        var cursorMock = new Mock<IAsyncCursor<WorkoutSession>>();
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(new[] { session });
        _workoutSessionCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<FindOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _workoutSessionRepository.GetWorkSessionById(memberId, sessionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(sessionId, result.SessionId);
    }

    [TestMethod]
    public async Task GetAllWorkSessionsByMemberId_ShouldReturnAllSessionsForMember()
    {
        // Arrange
        var memberId = 1;
        var sessions = new List<WorkoutSession> { new WorkoutSession { MemberId = memberId }, new WorkoutSession { MemberId = memberId } };
        var cursorMock = new Mock<IAsyncCursor<WorkoutSession>>();
        cursorMock.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true).ReturnsAsync(false);
        cursorMock.SetupGet(c => c.Current).Returns(sessions);
        _workoutSessionCollectionMock.Setup(c => c.FindAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<FindOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cursorMock.Object);

        // Act
        var result = await _workoutSessionRepository.GetAllWorkSessionsByMemberId(memberId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public async Task UpdateWorkSession_ShouldReturnUpdatedSession_WhenUpdateIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var updatedSession = new WorkoutSession { MemberId = memberId, SessionId = sessionId, Title = "Updated" };
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndReplaceAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), updatedSession, It.IsAny<FindOneAndReplaceOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedSession);

        // Act
        var result = await _workoutSessionRepository.UpdateWorkSession(memberId, sessionId, updatedSession);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(updatedSession.Title, result.Title);
    }

    [TestMethod]
    public async Task DeleteWorkSession_ShouldReturnDeletedSession_WhenDeleteIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var session = new WorkoutSession { MemberId = memberId, SessionId = sessionId };
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndDeleteAsync(It.IsAny<FilterDefinition<WorkoutSession>>(), It.IsAny<FindOneAndDeleteOptions<WorkoutSession, WorkoutSession>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(session);

        // Act
        var result = await _workoutSessionRepository.DeleteWorkSession(memberId, sessionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(sessionId, result.SessionId);
    }
}