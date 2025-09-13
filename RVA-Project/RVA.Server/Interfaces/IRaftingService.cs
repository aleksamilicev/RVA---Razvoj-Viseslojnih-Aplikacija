using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Server.Services
{
    /// WCF Service Contract for Rafting operations
    [ServiceContract]
    public interface IRaftingService
    {
        [OperationContract]
        IEnumerable<RaftingDto> GetAll();

        [OperationContract]
        RaftingDto GetById(int id);

        [OperationContract]
        int Create(RaftingDto rafting);

        [OperationContract]
        bool Update(RaftingDto rafting);

        [OperationContract]
        bool Delete(int id);

        [OperationContract]
        IEnumerable<RaftingDto> GetByState(RaftingState state);

        [OperationContract]
        IEnumerable<RaftingDto> GetActive();

        [OperationContract]
        IEnumerable<RaftingDto> GetAvailableForBooking();

        [OperationContract]
        bool ChangeState(int raftingId, RaftingState newState);

        [OperationContract]
        Dictionary<RaftingState, int> GetStateStatistics();

        [OperationContract]
        IEnumerable<RaftingDto> GetByDateRange(DateTime startDate, DateTime endDate);

        [OperationContract]
        IEnumerable<RaftingDto> GetByLocation(int locationId);

        [OperationContract]
        IEnumerable<RaftingDto> GetByPriceRange(decimal minPrice, decimal maxPrice);

        [OperationContract]
        ValidationResult Validate(RaftingDto rafting);
    }
}
