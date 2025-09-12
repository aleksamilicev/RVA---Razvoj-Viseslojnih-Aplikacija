using RVA.Shared.Enums;
using System;
using System.Collections.Generic;

namespace RVA.Shared.Models
{
    public class Rafting : WaterActivity
    {
        // Osnovne properties iz UML-a
        public DateTime EndTime { get; set; }
        public Intensity CurrentIntensity { get; set; }
        public double CurrentSpeedKmh { get; set; }
        public int Capacity { get; set; }

        // Dodane properties
        public RaftingState CurrentState { get; set; } = RaftingState.Planned;
        public int GuideId { get; set; }
        public decimal PricePerPerson { get; set; }
        public string WeatherConditions { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation properties - Relacije iz UML-a
        public int StartLocationId { get; set; }
        public Location StartLocation { get; set; } = new Location();

        public int EndLocationId { get; set; }
        public Location EndLocation { get; set; } = new Location();

        public List<Clothing> UsedClothing { get; set; } = new List<Clothing>();
        public List<Equipment> UsedEquipment { get; set; } = new List<Equipment>();


        /* Prebaciti u RVA.Server/Services
         * 
         * 
         * 
        // State Machine metode
        public void ChangeState(RaftingState newState)
        {
            if (CanTransitionTo(newState))
            {
                CurrentState = newState;
                UpdateIntensityByState();
                UpdateSpeedByState();
                ModifiedDate = DateTime.Now;
            }
        }

        public void UpdateIntensityByState()
        {
            CurrentIntensity = CurrentState switch
            {
                RaftingState.Planned => Intensity.Low,
                RaftingState.Boarding => Intensity.Low,
                RaftingState.Paddling => Intensity.High,
                RaftingState.Resting => Intensity.Medium,
                RaftingState.Finished => Intensity.Low,
                _ => Intensity.Medium
            };
        }

        public void UpdateSpeedByState()
        {
            CurrentSpeedKmh = CurrentState switch
            {
                RaftingState.Planned => 0.0,
                RaftingState.Boarding => 0.0,
                RaftingState.Paddling => 15.0,
                RaftingState.Resting => 5.0,
                RaftingState.Finished => 0.0,
                _ => 0.0
            };
        }

        // Business logic metode
        public DateTime CalculateEstimatedEndTime()
        {
            var estimatedDuration = Distance / CurrentSpeedKmh; // sati
            return StartTime.AddHours(estimatedDuration);
        }

        public decimal CalculateTotalPrice()
        {
            return PricePerPerson * ParticipantCount;
        }

        public bool ValidateCapacity()
        {
            return ParticipantCount <= Capacity && Capacity > 0;
        }

        public List<Equipment> GetAvailableEquipment()
        {
            return UsedEquipment.Where(e => e.IsAvailable).ToList();
        }

        public void AssignClothing(Clothing clothing)
        {
            if (clothing != null && clothing.IsAvailable && !UsedClothing.Contains(clothing))
            {
                UsedClothing.Add(clothing);
                clothing.MarkAsUsed();
            }
        }

        public void RemoveClothing(Clothing clothing)
        {
            if (clothing != null && UsedClothing.Contains(clothing))
            {
                UsedClothing.Remove(clothing);
                clothing.MarkAsAvailable();
            }
        }

        // Abstract metode implementacije
        public override TimeSpan CalculateDuration()
        {
            return EndTime - StartTime;
        }

        public override bool ValidateActivity()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   StartTime < EndTime &&
                   Distance > 0 &&
                   Capacity > 0 &&
                   StartLocation != null &&
                   EndLocation != null &&
                   UsedClothing.Count > 0; // Mora imati barem jedan deo odece
        }

        // Helper metode
        private bool CanTransitionTo(RaftingState newState)
        {
            return (CurrentState, newState) switch
            {
                (RaftingState.Planned, RaftingState.Boarding) => true,
                (RaftingState.Boarding, RaftingState.Paddling) => true,
                (RaftingState.Paddling, RaftingState.Resting) => true,
                (RaftingState.Resting, RaftingState.Paddling) => true,
                (RaftingState.Paddling, RaftingState.Finished) => true,
                (RaftingState.Resting, RaftingState.Finished) => true,
                _ => false
            };
        }

        public Rafting Clone()
        {
            return new Rafting
            {
                Id = this.Id,
                Name = this.Name,
                Description = this.Description,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                Duration = this.Duration,
                Distance = this.Distance,
                CurrentIntensity = this.CurrentIntensity,
                CurrentSpeedKmh = this.CurrentSpeedKmh,
                Capacity = this.Capacity,
                CurrentState = this.CurrentState,
                GuideId = this.GuideId,
                PricePerPerson = this.PricePerPerson,
                WeatherConditions = this.WeatherConditions,
                ParticipantCount = this.ParticipantCount,
                MaxParticipants = this.MaxParticipants,
                StartLocationId = this.StartLocationId,
                EndLocationId = this.EndLocationId,
                CreatedDate = this.CreatedDate,
                ModifiedDate = this.ModifiedDate
            };
        }
        */
    }
}
