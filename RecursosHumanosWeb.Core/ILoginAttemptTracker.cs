namespace RecursosHumanosWeb.Services;

public interface ILoginAttemptTracker
{
    bool IsLockedOut(string key);
    void RegisterFailure(string key);
    void Reset(string key);
}
