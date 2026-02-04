namespace PersonalFinanceManager.Interfaces;

public interface IConfig
{
    public string ConfigType { get; set; }
    public string Version { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IConfigService
{
    IConfig? GetConfig(string configType);
}