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
    }
}