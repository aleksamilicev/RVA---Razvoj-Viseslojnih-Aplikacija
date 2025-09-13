using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RVA.Server.Data
{
    /// Repository za Rafting entitete
    public class RaftingRepository : BaseRepository<RaftingDto>
    {
        public RaftingRepository(IDataStorage dataStorage, ILogger logger)
            : base(dataStorage, logger, "rafting_data")
        {
        }

        protected override int GetEntityId(RaftingDto entity)
        {
            return entity.Id;
        }

        protected override void SetEntityId(RaftingDto entity, int id)
        {
            entity.Id = id;
            entity.ModifiedDate = DateTime.Now;
            if (entity.CreatedDate == default(DateTime))
            {
                entity.CreatedDate = DateTime.Now;
            }
        }

         
        /// Pronalazi raftings po stanju
        public IEnumerable<RaftingDto> GetByState(RaftingState state)
        {
            return Find(r => r.CurrentState == state);
        }

         
        /// Pronalazi raftings po guide ID
        public IEnumerable<RaftingDto> GetByGuideId(int guideId)
        {
            return Find(r => r.GuideId == guideId);
        }

         
        /// Pronalazi raftings u određenom vremenskom opsegu
        public IEnumerable<RaftingDto> GetByDateRange(DateTime startDate, DateTime endDate)
        {
            return Find(r => r.StartTime >= startDate && r.StartTime <= endDate);
        }

         
        /// Pronalazi aktivne raftings (u toku)
        public IEnumerable<RaftingDto> GetActive()
        {
            var activeStates = new[] { RaftingState.Boarding, RaftingState.Paddling, RaftingState.Resting };
            return Find(r => activeStates.Contains(r.CurrentState));
        }

         
        /// Pronalazi raftings po lokaciji
        public IEnumerable<RaftingDto> GetByLocation(int startLocationId, int? endLocationId = null)
        {
            if (endLocationId.HasValue)
            {
                return Find(r => r.StartLocationId == startLocationId && r.EndLocationId == endLocationId.Value);
            }
            return Find(r => r.StartLocationId == startLocationId || r.EndLocationId == startLocationId);
        }

         
        /// Pronalazi raftings po intensitetu
        public IEnumerable<RaftingDto> GetByIntensity(Intensity intensity)
        {
            return Find(r => r.CurrentIntensity == intensity);
        }

         
        /// Pronalazi raftings koji još uvek primaju učesnike
        public IEnumerable<RaftingDto> GetAvailableForBooking()
        {
            return Find(r => r.CurrentState == RaftingState.Planned && r.ParticipantCount < r.MaxParticipants);
        }

         
        /// Pronalazi raftings po opsegu cena
        public IEnumerable<RaftingDto> GetByPriceRange(decimal minPrice, decimal maxPrice)
        {
            return Find(r => r.PricePerPerson >= minPrice && r.PricePerPerson <= maxPrice);
        }

         
        /// Vraća statistike po stanjima
        public Dictionary<RaftingState, int> GetStateStatistics()
        {
            var allRaftings = GetAll();
            return Enum.GetValues(typeof(RaftingState))
                      .Cast<RaftingState>()
                      .ToDictionary(state => state,
                                   state => allRaftings.Count(r => r.CurrentState == state));
        }
    }
}