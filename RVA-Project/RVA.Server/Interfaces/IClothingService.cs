using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RVA.Server.Interfaces
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
