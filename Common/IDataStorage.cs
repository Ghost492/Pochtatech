namespace Common
{
    public interface IDataStorage<T>
    {
        T Get(object key);
        void Create(T data);
        void Update(T data);
        void Delete(T data);
    }
}
