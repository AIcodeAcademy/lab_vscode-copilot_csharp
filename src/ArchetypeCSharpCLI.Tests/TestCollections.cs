using Xunit;

namespace ArchetypeCSharpCLI.Tests;

/// <summary>
/// Non-parallel collection for tests that write/read shared appsettings files.
/// </summary>
[CollectionDefinition("ConfigFiles", DisableParallelization = true)]
public class ConfigFilesCollection { }
