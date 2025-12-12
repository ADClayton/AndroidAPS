namespace AndroidAPSMaui.Services;

public class ServiceResolver
{
    private static IServiceProvider? _serviceProvider;

    public static void Initialize(IServiceProvider provider) => _serviceProvider = provider;

    public static T? Resolve<T>() where T : class
    {
        return _serviceProvider?.GetService<T>();
    }
}
