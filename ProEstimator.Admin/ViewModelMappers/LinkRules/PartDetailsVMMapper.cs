using System.Data;

using Proestimator.Admin.ViewModelMappers;
using ProEstimator.Admin.ViewModel.LinkRules;
using ProEstimatorData;

namespace ProEstimator.Admin.ViewModelMappers.LinkRules
{
    public class PartDetailsVMMapper : IVMMapper<PartDetailsVM>
    {
        public PartDetailsVM Map(MappingConfiguration mappingConfiguration)
        {
            PartDetailsVMMapperConfiguration config = mappingConfiguration as PartDetailsVMMapperConfiguration;
            DataRow row = config.Row;

            PartDetailsVM vm = new PartDetailsVM
            {
                PartNumber = InputHelper.GetString(row["PartNumber"].ToString()),
                Description = InputHelper.GetString(row["Description"].ToString()),
                Comment = InputHelper.GetString(row["comment"].ToString()),
                Price = InputHelper.GetDouble(row["Price"].ToString()),
                Barcode = InputHelper.GetString(row["Barcode"].ToString()),
                PartText = InputHelper.GetString(row["Part_Text"].ToString()),
                Notes = InputHelper.GetString(row["Notes"].ToString())
            };

            return vm;
        }
    }

    public class PartDetailsVMMapperConfiguration : MappingConfiguration
    {
        public DataRow Row { get; set; }
    }
}