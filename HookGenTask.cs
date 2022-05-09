using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil.Cil;
using MonoMod;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;

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
        
        Environment.SetEnvironmentVariable("MONOMOD_HOOKGEN_PRIVATE", "1");
        Environment.SetEnvironmentVariable("MONOMOD_DEPENDENCY_MISSING_THROW", "0");

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
