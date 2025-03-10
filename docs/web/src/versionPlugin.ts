import { defineRouteMiddleware } from "@astrojs/starlight/route-data";
import { type StarlightPlugin } from "@astrojs/starlight/types";

const version = process.env.CONTEXTIVE_VERSION;
const archive = !!process.env.CONTEXTIVE_ARCHIVE;
const sha = process.env.CONTEXTIVE_SHA;
const release_label = `Release: v${version}`;
const release_url = sha !== '' ?
    `https://github.com/dev-cycles/contextive/commit/${sha}`
    : `https://github.com/dev-cycles/contextive/releases/tag/v${version}`;
const release_link =
{
    link: release_url,
    attrs: { target: '_blank' },
}

const ROUTE_TO_CURRENT_VERSION = "/ide";

const versionBannerMiddleware = defineRouteMiddleware(context => {
    // Get the content collection entry for this page.
    const { entry } = context.locals.starlightRoute;
    // Update the title to add an exclamation mark.
    entry.data.contextive_version = version;
    if (archive) {
        entry.data.banner = { content: `<p>You are reading the documentation for version <span class="version">${version}</span>.</p><p><a href="${ROUTE_TO_CURRENT_VERSION}">Documentation for the latest version</a>.</p>` }
    }
    if (entry.filePath === 'src/content/docs/index.mdx') {
        entry.data.hero?.actions.push({
            text: release_label,
            ...release_link,
            variant: 'secondary',
            icon: { type: 'icon', name: 'github' }
        })
    }
});

export default function () {
    return {
        name: 'Version Banner',
        hooks: {
            'config:setup': ({ config, updateConfig, addRouteMiddleware }) => {
                if (version) {
                    const { title, logo, sidebar } = config;
                    sidebar?.unshift({
                        label: release_label,
                        ...release_link,
                        attrs: { target: '_blank' },
                    })
                    updateConfig({
                        title: title + ' - ' + version,
                        logo: {
                            ...logo,
                            alt: logo?.alt + ' - ' + version
                        } as typeof logo,
                        sidebar,
                    });
                    addRouteMiddleware({ entrypoint: import.meta.filename, order: 'pre' })
                }
            },
        }
    } as StarlightPlugin;
};

export const onRequest = versionBannerMiddleware