using Microsoft.Build.Framework;

namespace HookGen.MSBuild;

public sealed class GameLibsPackage
{
    public string Id { get; }
    public string DummyDirectory { get; }
    public string Version { get; }
    public string[] HookGenList { get; }

    public GameLibsPackage(ITaskItem item)
    {
        Id = item.ItemSpec;
        Version = item.GetMetadata("Version");
        DummyDirectory = item.GetMetadata("DummyDirectory");
        HookGenList = item.GetMetadata("HookGenList").Split(',').Select(x => Path.Combine(DummyDirectory, x)).ToArray();
    }
}
