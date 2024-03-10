using System.Data;

namespace ProEstimator.Business.ILogic
{
    public interface IModelMap<T>
    {
        T ToModel(DataRow row);
    }
}
