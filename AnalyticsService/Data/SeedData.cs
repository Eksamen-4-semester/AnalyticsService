using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AnalyticsService.Models;

namespace AnalyticsService.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IMongoDatabase database)
    {
        var collection = database.GetCollection<WorkoutSession>("WorkoutSessions");

        if (await collection.Find(_ => true).AnyAsync())
        {
            return;
        }

        var sessions = new List<WorkoutSession>();
        int sessionIdCounter = 1;
        int exerciseIdCounter = 1;
        int setIdCounter = 1;
        var random = new Random();
        
        int currentYear = 2024; 
        
        for (int year = currentYear - 3; year <= currentYear; year++)
        {
            var weeklySchedule = new[]
            {
                new { Day = DayOfWeek.Monday, Title = "Bryst dag", MuscleGroup = MuscleGroup.Chest },
                new { Day = DayOfWeek.Tuesday, Title = "Ben dag", MuscleGroup = MuscleGroup.Legs },
                new { Day = DayOfWeek.Thursday, Title = "Ryg dag", MuscleGroup = MuscleGroup.Back },
                new { Day = DayOfWeek.Friday, Title = "Skulder dag", MuscleGroup = MuscleGroup.Shoulders }
            };

            for (int month = 1; month <= 12; month++)
            {
                if (year == currentYear && month > DateTime.UtcNow.Month)
                {
                    continue;
                }

                int daysInMonth = DateTime.DaysInMonth(year, month);
                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day, 8, 0, 0);
                    var scheduleItem = Array.Find(weeklySchedule, s => s.Day == date.DayOfWeek);

                    if (scheduleItem != null)
                    {
                        if (random.Next(10) < 1) continue;

                        var exercises = GetExercisesForMuscleGroup(scheduleItem.MuscleGroup, year, ref exerciseIdCounter, ref setIdCounter);
                        
                        int duration = random.Next(55, 85);
                        sessions.Add(new WorkoutSession
                        {
                            SessionId = sessionIdCounter++,
                            MemberId = 1,
                            Title = scheduleItem.Title,
                            MuscleGroup = scheduleItem.MuscleGroup,
                            Date = date,
                            StartedAt = date,
                            EndedAt = date.AddMinutes(duration),
                            DurationMinutes = duration,
                            Exercises = exercises
                        });
                    }
                }
            }
        }

        if (sessions.Count > 0)
        {
            await collection.InsertManyAsync(sessions);
        }
    }

    private static List<SessionExercise> GetExercisesForMuscleGroup(MuscleGroup group, int year, ref int exerciseIdCounter, ref int setIdCounter)
    {
        var exercises = new List<SessionExercise>();
        decimal progressionFactor = 1 + ((year - 2021) * 0.05m);

        switch (group)
        {
            case MuscleGroup.Chest:
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Bænkpres", Order = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 8, WeightKg = Math.Round(70.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 8, WeightKg = Math.Round(75.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 3, Reps = 6, WeightKg = Math.Round(80.0m * progressionFactor, 1) }
                    }
                });
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Incline Dumbbell Press", Order = 2,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 12, WeightKg = Math.Round(26.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 10, WeightKg = Math.Round(28.0m * progressionFactor, 1) }
                    }
                });
                break;
            case MuscleGroup.Legs:
                 exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Squat", Order = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 8, WeightKg = Math.Round(90.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 6, WeightKg = Math.Round(100.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 3, Reps = 4, WeightKg = Math.Round(110.0m * progressionFactor, 1) }
                    }
                });
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Leg Press", Order = 2,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 12, WeightKg = Math.Round(140.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 10, WeightKg = Math.Round(150.0m * progressionFactor, 1) }
                    }
                });
                break;
            case MuscleGroup.Back:
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Dødløft", Order = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 5, WeightKg = Math.Round(110.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 3, WeightKg = Math.Round(120.0m * progressionFactor, 1) }
                    }
                });
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Pullups", Order = 2,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 10, WeightKg = 0.0m },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 8, WeightKg = 0.0m }
                    }
                });
                break;
            case MuscleGroup.Shoulders:
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Overhead Press", Order = 1,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 10, WeightKg = Math.Round(45.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 8, WeightKg = Math.Round(50.0m * progressionFactor, 1) }
                    }
                });
                exercises.Add(new SessionExercise
                {
                    ExerciseId = exerciseIdCounter++, ExerciseName = "Lateral Raises", Order = 2,
                    Sets = new List<ExerciseSet>
                    {
                        new() { SetId = setIdCounter++, SetNumber = 1, Reps = 15, WeightKg = Math.Round(8.0m * progressionFactor, 1) },
                        new() { SetId = setIdCounter++, SetNumber = 2, Reps = 12, WeightKg = Math.Round(10.0m * progressionFactor, 1) }
                    }
                });
                break;
        }
        return exercises;
    }
}
