using System.Collections.Generic;

namespace ProEstimator.Business.ILogic
{
    public interface IDataTableMap <T>
    {
        T MapFromDataTableRow(Dictionary<string, Dictionary<string, string>> row);
        Dictionary<string, Dictionary<string, string>> MapToDataTableRow();
    }
}
