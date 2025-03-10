import { defineRouteMiddleware } from "@astrojs/starlight/route-data";
import { type StarlightPlugin } from "@astrojs/starlight/types";

const version = process.env.VERSION;

const ROUTE_TO_CURRENT_VERSION = "/ide";

const versionBannerMiddleware = defineRouteMiddleware(context => {
    // Get the content collection entry for this page.
    const { entry } = context.locals.starlightRoute;
    // Update the title to add an exclamation mark.
    entry.data.banner = { content: `<p>You are reading the documentation for version <span class="version">${version}</span>.</p><p><a href="${ROUTE_TO_CURRENT_VERSION}">Documentation for the latest version</a>.</p>` }
});

export default function () {
    return {
        name: 'Version Banner',
        hooks: {
            'config:setup': ({ config, updateConfig, addRouteMiddleware }) => {
                if (version) {
                    const { title, logo } = config;
                    updateConfig({
                        title: title + ' - ' + version,
                        logo: {
                            ...logo,
                            alt: logo?.alt + ' - ' + version
                        } as typeof logo
                    });
                    addRouteMiddleware({ entrypoint: import.meta.filename, order: 'pre' })
                }
            },
        }
    } as StarlightPlugin;
};

export const onRequest = versionBannerMiddleware