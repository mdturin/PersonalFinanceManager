using PersonalFinanceManager.Core.Interfaces;

namespace PersonalFinanceManager.Core.Configurations;

public class NavItem
{
    public string Label { get; set; }
    public string Route { get; set; }
    public bool Active { get; set; }
    public string Icon { get; set; }
}

public class NavSection
{
    public string Title { get; set; }
    public List<NavItem> Items { get; set; }
}

public class NavCardItem
{
    public string Title { get; set; }
    public string Body { get; set; }
    public string ButtonLabel { get; set; }
    public string ButtonAction { get; set; }
    public bool Hidden { get; set; }
}

public class ConfigBase : IConfig
{
    public string ConfigType { get; set; }
    public string Version { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class SideNavConfig : ConfigBase
{
    public List<NavSection> Sections { get; set; }
    public List<NavCardItem> Cards { get; set; }

    public SideNavConfig()
    {
        ConfigType = "side-nav";
    }
}