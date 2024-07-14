using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Microsoft.VisualStudio.RpcContracts.LanguageServerProvider;
using Nerdbank.Streams;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Reflection;

namespace contextive;

[VisualStudioContribution]
public class ContextiveLanguageServerProvider : LanguageServerProvider
{
    public ContextiveLanguageServerProvider(
        ExtensionCore container,
        VisualStudioExtensibility extensibilityObject,
        TraceSource traceSource) : base(container, extensibilityObject)
    {

    }

    public IEnumerable<string> ConfigurationSections
    {
        get
        {
            yield return "contextive";
        }
    }

    [VisualStudioContribution]
    public static DocumentTypeConfiguration AnyDocumentType => new("any")
    {
        FileExtensions = new[] { ".*" },
        BaseDocumentType = LanguageServerBaseDocumentType,
    };

    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration =>
        new("%contextive.LanguageServerProvider.DisplayName%",
            new[]
            {
                DocumentFilter.FromDocumentType(AnyDocumentType),
            });

    public override Task<IDuplexPipe?> CreateServerConnectionAsync(CancellationToken cancellationToken)
    {
        string workingDirectory = Path.Combine(
            Path.GetTempPath(),
            "contextive",
            Guid.NewGuid().ToString());
        Directory.CreateDirectory(workingDirectory);

        ProcessStartInfo info = new()
        {
            FileName = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
                @"Contextive.LanguageServer.exe"),
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            UseShellExecute = false, // needs to be false so that IO streams can be redirected
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory
        };

        Process process = new Process();
        process.StartInfo = info;

        if (process.Start())
        {
            return Task.FromResult<IDuplexPipe?>(new DuplexPipe(
                PipeReader.Create(process.StandardOutput.BaseStream),
                PipeWriter.Create(process.StandardInput.BaseStream)));
        }

        return Task.FromResult<IDuplexPipe?>(null);
    }

    public override Task OnServerInitializationResultAsync(
        ServerInitializationResult serverInitializationResult,
        LanguageServerInitializationFailureInfo? initializationFailureInfo,
        CancellationToken cancellationToken)
    {
        if (serverInitializationResult == ServerInitializationResult.Failed)
        {
            this.Enabled = false;
        }
        return base.OnServerInitializationResultAsync(
            serverInitializationResult, initializationFailureInfo, cancellationToken);
    }
}
