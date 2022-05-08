using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace HookGen.MSBuild;

public sealed class HookGenTask : Microsoft.Build.Utilities.Task
{
    [Required]
    public ITaskItem[] HookGen { get; set; } = null!;

    [Output]
    public ITaskItem[] HookGenedDlls { get; set; } = null!;
    
    public override bool Execute()
    {
        ConcurrentBag<ITaskItem> outputs = new();

        Parallel.ForEach(HookGen.Select(pkg => new GameLibsPackage(pkg)), package =>
        {
            foreach (var file in Directory.EnumerateFiles(new HookGenAssemblyGenerator(package, Log).Generate(), "*.dll"))
            {
                var taskItem = new TaskItem(file);
                taskItem.SetMetadata("PackageId", package.Id);
                
                outputs.Add(taskItem);
            }
        });

        HookGenedDlls = outputs.ToArray();
        
        return true;
    }
}
