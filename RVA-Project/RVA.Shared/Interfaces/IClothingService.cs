using RVA.Shared.DTOs;
using RVA.Shared.Enums;
using RVA.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using ValidationResult = RVA.Shared.DTOs.ValidationResult;

namespace RVA.Shared.Interfaces
{
    [ServiceContract]
    public interface IClothingService
    {
        [OperationContract]
        IEnumerable<ClothingDto> GetAll();

        [OperationContract]
        ClothingDto GetById(int id);

        [OperationContract]
        int Create(ClothingDto clothing);

        [OperationContract]
        bool Update(ClothingDto clothing);

        [OperationContract]
        bool Delete(int id);

        [OperationContract]
        IEnumerable<ClothingDto> GetAvailable();

        [OperationContract]
        IEnumerable<ClothingDto> GetByType(ClothingType type);

        [OperationContract]
        IEnumerable<ClothingDto> GetByMaterial(Material material);

        [OperationContract]
        IEnumerable<ClothingDto> GetWaterproof();

        [OperationContract]
        IEnumerable<ClothingDto> GetBySize(int size);

        [OperationContract]
        bool MarkAsUsed(int id);

        [OperationContract]
        bool MarkAsAvailable(int id);

        [OperationContract]
        ValidationResult Validate(ClothingDto clothing);
    }
}
