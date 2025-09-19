// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import versionPlugin from './src/versionPlugin';
import rehypeExternalLinks from 'rehype-external-links';
import { fromHtml } from 'hast-util-from-html';
import externalLinkIcon from './src/assets/icons/external.svg?raw'

const externalLinkElement = fromHtml(externalLinkIcon, { fragment: true, space: 'svg' });

// https://astro.build/config
export default defineConfig({
    base: process.env.BASE_URL,
    redirects: {
        "/guides/usage": "/guides/defining-terminology/"
    },
    markdown: {
        rehypePlugins: [
            [
                rehypeExternalLinks,
                {
                    content: externalLinkElement,
                    target: "_blank"
                }
            ],
        ]
    },
    integrations: [starlight({
        title: 'Contextive - Community Edition',
        description: 'Get on the same page.',
        editLink: {
            baseUrl: 'https://github.com/dev-cycles/contextive/edit/main/docs/web/',
        },
        logo: {
            alt: 'Contextive',
            replacesTitle: true,
            light: './src/assets/logos/logo-primary.png',
            dark: './src/assets/logos/logo-inverted.png'
        },
        social: {
            github: 'https://github.com/dev-cycles/contextive',
            blueSky: 'https://bsky.app/profile/contextive.tech',
            linkedin: 'https://www.linkedin.com/company/contextive-tech',
        },
        customCss: [
            './src/assets/fonts/Forrest/font-face.css',
            '@fontsource/atkinson-hyperlegible/400.css',
            './src/styles/custom.css',
        ],
        sidebar: [
            {
                label: 'Guides',
                autogenerate: { directory: 'guides' },
            },
            {
                label: 'Background',
                autogenerate: { directory: 'background' }
            },
            // {
            // 	label: 'Reference',
            // 	autogenerate: { directory: 'reference' },
            // },
            'changelog',
        ],
        plugins: [
            versionPlugin,
        ],
    })],
});