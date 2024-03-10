namespace ProEstimator.Business.ILogic
{
    public interface IMapper<T>
    {
        Y Convert<Y>(T item) where Y : new();
        void MapTo<Y>(Y src, T dest);
        void MapFrom<Y>(T src, Y dest);
        T Generate<Y>(Y item);
    }
}
