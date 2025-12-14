namespace Utils
{
    public interface IUpdateObserver
    {
        void OnUpdate();
    }

    public interface IFixedUpdateObserver
    {
        void OnFixedUpdate();
    }

    public interface ILateUpdateObserver
    {
        void OnLateUpdate();
    }
}