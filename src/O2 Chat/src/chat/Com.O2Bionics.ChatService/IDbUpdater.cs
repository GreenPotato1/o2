namespace Com.O2Bionics.ChatService
{
    public interface IDbUpdater
    {
        void Load();
        void Update();
        void Start();
        void Stop();
    }
}