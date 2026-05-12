using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace AnalyticsService.Models;

public class WorkoutSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public int SessionId { get; set; }
    public int MemberId { get; set; }
    public string Title { get; set; } = string.Empty;
    public MuscleGroup MuscleGroup { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public List<SessionExercise> Exercises { get; set; } = new();
}
public enum MuscleGroup
{
    Chest,
    Back,
    Legs,
    Shoulders,
    Arms,
    Core,
    FulLBody,
    Cardio
}
