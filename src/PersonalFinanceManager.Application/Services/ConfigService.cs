using PersonalFinanceManager.Configurations;
using PersonalFinanceManager.Data;
using PersonalFinanceManager.Interfaces;

namespace PersonalFinanceManager.Services;

public class ConfigService(LiteDbContext liteDbContext) : IConfigService
{
    private T? GetItem<T>(string configType) where T:ConfigBase
    {
        return liteDbContext
            .GetItemByQuery<T>(config => config.ConfigType == configType, false, nameof(ConfigBase.Version));
    }
    
    public IConfig? GetConfig(string configType)
    {
        return configType switch
        {
            "side-nav" => GetItem<SideNavConfig>(configType),
            _ => throw new NotImplementedException()
        };
    }
}