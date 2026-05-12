using AnalyticsService.Models;
using AnalyticsService.Repositories;
using MongoDB.Driver;
using Moq;

namespace AnalyticsService.Tests;

[TestClass]
public class ExerciseSetRepositoryMongoDbTests
{
    private Mock<IMongoDatabase> _mongoDatabaseMock = null!;
    private Mock<IMongoCollection<WorkoutSession>> _workoutSessionCollectionMock = null!;
    private ExerciseSetRepositoryMongoDb _exerciseSetRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mongoDatabaseMock = new Mock<IMongoDatabase>();
        _workoutSessionCollectionMock = new Mock<IMongoCollection<WorkoutSession>>();
        _mongoDatabaseMock.Setup(db => db.GetCollection<WorkoutSession>("WorkoutSessions", null))
            .Returns(_workoutSessionCollectionMock.Object);
        _exerciseSetRepository = new ExerciseSetRepositoryMongoDb(_mongoDatabaseMock.Object);
    }

    [TestMethod]
    public async Task AddSetToExercise_ShouldReturnSet_WhenUpdateIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var exerciseId = 1;
        var set = new ExerciseSet { SetId = 1 };
        var sessionWithSet = new WorkoutSession 
        { 
            MemberId = memberId, 
            SessionId = sessionId, 
            Exercises = new List<SessionExercise> 
            { 
                new SessionExercise { ExerciseId = exerciseId, Sets = new List<ExerciseSet> { set } } 
            } 
        };
        
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<WorkoutSession>>(), 
            It.IsAny<UpdateDefinition<WorkoutSession>>(), 
            It.IsAny<FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionWithSet);

        // Act
        var result = await _exerciseSetRepository.AddSetToExercise(memberId, sessionId, exerciseId, set);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(set.SetId, result.SetId);
    }

    [TestMethod]
    public async Task UpdateSetInExercise_ShouldReturnUpdatedSet_WhenUpdateIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var exerciseId = 1;
        var setId = 1;
        var updatedSet = new ExerciseSet { SetId = 1, Reps = 12 };
        var sessionWithUpdatedSet = new WorkoutSession 
        { 
            MemberId = memberId, 
            SessionId = sessionId, 
            Exercises = new List<SessionExercise> 
            { 
                new SessionExercise { ExerciseId = exerciseId, Sets = new List<ExerciseSet> { updatedSet } } 
            } 
        };
        
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<WorkoutSession>>(), 
            It.IsAny<UpdateDefinition<WorkoutSession>>(), 
            It.IsAny<FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionWithUpdatedSet);

        // Act
        var result = await _exerciseSetRepository.UpdateSetInExercise(memberId, sessionId, exerciseId, setId, updatedSet);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(updatedSet.Reps, result.Reps);
    }

    [TestMethod]
    public async Task DeleteSetFromExercise_ShouldReturnDeletedSet_WhenDeleteIsSuccessful()
    {
        // Arrange
        var memberId = 1;
        var sessionId = 1;
        var exerciseId = 1;
        var setId = 1;
        var setToDelete = new ExerciseSet { SetId = setId };
        var sessionBeforeDelete = new WorkoutSession 
        { 
            MemberId = memberId, 
            SessionId = sessionId, 
            Exercises = new List<SessionExercise> 
            { 
                new SessionExercise { ExerciseId = exerciseId, Sets = new List<ExerciseSet> { setToDelete } } 
            } 
        };
        
        _workoutSessionCollectionMock.Setup(c => c.FindOneAndUpdateAsync(
            It.IsAny<FilterDefinition<WorkoutSession>>(), 
            It.IsAny<UpdateDefinition<WorkoutSession>>(), 
            It.IsAny<FindOneAndUpdateOptions<WorkoutSession, WorkoutSession>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionBeforeDelete);

        // Act
        var result = await _exerciseSetRepository.DeleteSetFromExercise(memberId, sessionId, exerciseId, setId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(setId, result.SetId);
    }
}