namespace GitProfileManager.Services;

public class ServiceImplementation(ILogger<ServiceImplementation> logger) : IService
{
    public void DoSomething()
    {
        logger.LogCritical("I am doing something");
    }
}