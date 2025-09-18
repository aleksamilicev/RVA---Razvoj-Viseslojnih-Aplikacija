using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RVA.Server.Data
{
    public class ClothingRepository : BaseRepository<ClothingDto>
    {
        public ClothingRepository(IDataStorage dataStorage, ILogger logger)
            : base(dataStorage, logger, "clothing_data")
        {
        }

        protected override int GetEntityId(ClothingDto entity)
        {
            return entity.Id;
        }

        protected override void SetEntityId(ClothingDto entity, int id)
        {
            entity.Id = id;
            if (entity.LastCleaned == default(DateTime))
            {
                entity.LastCleaned = DateTime.Now;
            }
        }

        /// Pronalazi odeću po tipu (Jacket, Suit, Helmet...)
        public IEnumerable<ClothingDto> GetByType(ClothingType type)
        {
            return Find(c => c.Type == type);
        }

        /// Pronalazi odeću određenog brenda
        public IEnumerable<ClothingDto> GetByBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand))
                return Enumerable.Empty<ClothingDto>();

            return Find(c => c.Brand.ToLower().Contains(brand.ToLower()));
        }

        /// Filtrira odeću po dostupnosti
        public IEnumerable<ClothingDto> GetAvailable()
        {
            return Find(c => c.IsAvailable);
        }

        /// Filtrira odeću koja je vodootporna
        public IEnumerable<ClothingDto> GetWaterproof()
        {
            return Find(c => c.IsWaterproof);
        }

        /// Pronalazi odeću koja treba da se očisti (npr. nije čišćena duže od X dana)
        public IEnumerable<ClothingDto> GetNeedsCleaning(int daysThreshold)
        {
            var cutoff = DateTime.Now.AddDays(-daysThreshold);
            return Find(c => c.LastCleaned < cutoff);
        }

        /// Pretražuje odeću po imenu, brendu ili boji
        public IEnumerable<ClothingDto> SearchByText(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return GetAll();

            var lowerSearch = searchText.ToLower();
            return Find(c => c.Name.ToLower().Contains(lowerSearch) ||
                             c.Brand.ToLower().Contains(lowerSearch) ||
                             c.Color.ToLower().Contains(lowerSearch));
        }
    }
}