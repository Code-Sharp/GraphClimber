namespace GraphClimber.Examples
{
    public interface IStore
    {

        IStore GetInner(string path);

        void Set<T>(string path, T value);

        bool TryGet<T>(string path, out T value);

    }
}