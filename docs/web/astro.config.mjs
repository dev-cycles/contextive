// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// https://astro.build/config
export default defineConfig({
	integrations: [
		starlight({
			title: 'Contextive',
			logo: {
				alt: 'Contextive',
				replacesTitle: true,
				light: './src/assets/logos/logo-primary.png',
				dark: './src/assets/logos/logo-inverted.png'
			},
			social: {
				github: 'https://github.com/dev-cycles/contextive',
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
