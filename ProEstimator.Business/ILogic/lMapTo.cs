using System.Data;

namespace ProEstimator.Business.ILogic
{
    public interface IMapTo<T>
    {
        T ToModel(T to);
        //T ToModel<T>() where T : new();
    }
}
