namespace ArchetypeCSharpCLI.Commands.Hello;

/// <summary>
/// Input options for the <c>hello</c> subcommand.
/// </summary>
public sealed class HelloOptions
{
    /// <summary>
    /// The name to greet. Must be non-empty/whitespace.
    /// </summary>
    public string Name { get; init; } = string.Empty;
}
