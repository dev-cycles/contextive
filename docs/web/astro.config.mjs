// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';
import packageInfo from './package.json';
import versionBannerPlugin from './src/versionBannerPlugin.ts';

const version = process.env.VERSION || packageInfo.version;

// https://astro.build/config
export default defineConfig({
	base: process.env.BASE_URL,
	integrations: [
		starlight({
			title: 'Contextive - IDE Extensions - ' + version,
			description: 'IDE Extensions to help immerse developers in the language of their domains.',
			editLink: {
				baseUrl: 'https://github.com/dev-cycles/contextive/edit/main/docs/web/',
			},
			tagline: 'Get on the same page.',
			logo: {
				alt: 'Contextive - ' + version,
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
					label: 'Version: ' + version,
					link: "/"
				},
				{
					label: 'Guides',
					autogenerate: { directory: 'guides' },
				},
				{
					label: 'Background',
					autogenerate: { directory: 'background' }
				}
				// {
				// 	label: 'Reference',
				// 	autogenerate: { directory: 'reference' },
				// },
			],
			plugins: [
				versionBannerPlugin(version)
			]
		}),
	],
});
