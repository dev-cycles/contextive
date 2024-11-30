// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Contextive',
			description: 'A suite of tools for immersing developers in the language of your domains.',
			editLink: {
				baseUrl: 'https://github.com/dev-cycles/contextive/edit/main/docs/web/',
			},
			tagline: 'Get on the same page.',
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
				}
				// {
				// 	label: 'Reference',
				// 	autogenerate: { directory: 'reference' },
				// },
			],
		}),
	],
});
