using RVA.Shared.DTOs;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Shared.Interfaces
{
    [ServiceContract]
    public interface ILocationService
    {
        [OperationContract]
        IEnumerable<LocationDto> GetAll();

        [OperationContract]
        LocationDto GetById(int id);

        [OperationContract]
        int Create(LocationDto location);

        [OperationContract]
        bool Update(LocationDto location);

        [OperationContract]
        bool Delete(int id);

        [OperationContract]
        IEnumerable<LocationDto> GetByRiver(string river);

        [OperationContract]
        IEnumerable<LocationDto> GetWithParking();

        [OperationContract]
        IEnumerable<LocationDto> GetWithFacilities();

        [OperationContract]
        IEnumerable<LocationDto> GetWithinRadius(double latitude, double longitude, double radiusKm);

        [OperationContract]
        IEnumerable<LocationDto> SearchByText(string searchText);

        [OperationContract]
        LocationDto GetNearest(double latitude, double longitude);

        [OperationContract]
        IEnumerable<string> GetUniqueRivers();

        [OperationContract]
        ValidationResult Validate(LocationDto location);
    }
}
