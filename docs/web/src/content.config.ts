import { defineCollection, z } from 'astro:content';
import { docsLoader } from "@astrojs/starlight/loaders";
import { docsSchema } from '@astrojs/starlight/schema';

export const collections = {
    docs: defineCollection({
        loader: docsLoader(), schema: docsSchema({
            extend: z.object({
                // Add a new field to the schema.
                contextive_version: z.string().optional(),
            }),
        })
    }),
};
