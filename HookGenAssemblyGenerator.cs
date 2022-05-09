using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Build.Utilities;
using Mono.Cecil;
using MonoMod;
using MonoMod.RuntimeDetour.HookGen;

namespace HookGen.MSBuild;

public sealed class HookGenAssemblyGenerator
{
    private GameLibsPackage Package { get; }
    private TaskLoggingHelper Log { get; }
    
    public HookGenAssemblyGenerator(GameLibsPackage package, TaskLoggingHelper log)
    {
        Package = package;
        Log = log;
    }
    
    public string Generate()
    {
        var outDir = Path.Combine(Context.CachePath, "game-libs", Package.Id, Package.Version, "hookgen");
        if (!Directory.Exists(outDir))
            Directory.CreateDirectory(outDir);

        var hashPath = Path.Combine(outDir, "hash.txt");
        var hash = ComputeHash(Package.DummyDirectory);

        if (File.Exists(hashPath) && File.ReadAllText(hashPath) == hash)
        {
            return outDir;
        }

        Parallel.ForEach(Package.HookGenList, asm =>
        {
            HookGen(asm, outDir);
        });
        
        //File.WriteAllText(hashPath, hash);

        return outDir;
    }

    private void HookGen(string asmPath, string outDir)
    {
        var outPath = Path.Combine(outDir, "MMHOOK_" + Path.GetFileName(asmPath));

        using MonoModder mm = new()
        {
            InputPath = asmPath,
            ReadingMode = ReadingMode.Deferred
        };

        mm.DependencyDirs.Add(Path.GetDirectoryName(typeof(HookGenAssemblyGenerator).Assembly.Location));

        mm.Read();
        mm.MapDependencies();
        
        if (File.Exists(outPath))
            File.Delete(outPath);

        HookGenerator gen = new(mm, outPath);
        using var mout = gen.OutputModule;
        gen.Generate();
        mout.Write(outPath);
    }

    private static string ComputeHash(string directory)
    {
        using var md5 = MD5.Create();
        foreach (var file in Directory.EnumerateFiles(directory, "*.dll"))
        {
            var pathBytes = Encoding.UTF8.GetBytes(Path.GetFileName(file));
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, null, 0);

            var contentBytes = File.ReadAllBytes(file);
            md5.TransformBlock(contentBytes, 0, contentBytes.Length, null, 0);
        }

        md5.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

        var bytes = md5.Hash;
        var builder = new StringBuilder(bytes.Length * 2);
        foreach (byte b in bytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
}
