using System.Reflection;
using System.Runtime.Versioning;

namespace TheDialgaTeam.Pokemon3D.Server.Core.Utilities;

public static class ApplicationUtility
{
    public static string Name { get; } = Assembly.GetEntryAssembly()!.GetCustomAttribute<AssemblyProductAttribute>()!.Product;

    public static string Version { get; } = Assembly.GetEntryAssembly()!.GetName().Version!.ToString();

    public static string FrameworkVersion { get; } = Assembly.GetEntryAssembly()!.GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName;
}