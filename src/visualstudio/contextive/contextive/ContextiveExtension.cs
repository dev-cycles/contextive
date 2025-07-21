using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

namespace contextive
{
    [VisualStudioContribution]
    internal class ContextiveExtension : Extension
    {
        /// <inheritdoc/>
        public override ExtensionConfiguration ExtensionConfiguration => new()
        {
            Metadata = new(
                    id: "Contextive.3b084294-49d4-487f-93f6-cadc0e419ac3",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Chris Simon",
                    displayName: "Contextive",
                    description: "Supports developers where a complex domain or project specific language is in use by surfacing definitions everywhere specific words are used - code, comments, config or documentation.")
            {
                // See https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.extensibility.extensionmetadata?view=vs-extensibility#properties
                MoreInfo = "https://contextive.tech",
                InstallationTargetArchitecture = VisualStudioArchitecture.Amd64,
                Icon = "contextive.png",
                License = "LICENSE",
                ReleaseNotes = "https://github.com/dev-cycles/contextive/releases/tag/v1.17.2",
                Tags = ["DDD", "Dictionary", "Domain-Driven Design", "Domain Modelling", "Knowledge Management"]
            }
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);
        }
    }
}
