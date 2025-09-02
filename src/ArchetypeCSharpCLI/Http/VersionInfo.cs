using System.Reflection;

namespace ArchetypeCSharpCLI.Http;

internal static class VersionInfo
{
  public static string GetInformationalVersion(Assembly? asm = null)
  {
    asm ??= Assembly.GetExecutingAssembly();
    var info = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    if (!string.IsNullOrWhiteSpace(info)) return info!;
    return asm.GetName().Version?.ToString() ?? "0.0.0";
  }
}
