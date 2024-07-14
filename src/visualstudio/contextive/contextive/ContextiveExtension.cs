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
                    id: "contextive.f050f6e5-c51b-4c33-9a59-ba9945a00259",
                    version: this.ExtensionAssemblyVersion,
                    publisherName: "Dev Cycles",
                    displayName: "Contextive",
                    description: "Supports developers where a complex domain or project specific language is in use by surfacing definitions everywhere specific words are used - code, comments, config or documentation."),
        };

        /// <inheritdoc />
        protected override void InitializeServices(IServiceCollection serviceCollection)
        {
            base.InitializeServices(serviceCollection);
        }
    }
}
