using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Application.Interfaces;

public interface IConfigService
{
    IConfig? GetConfig(string configType);
}