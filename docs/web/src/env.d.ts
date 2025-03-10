/// <reference path="../.astro/types.d.ts" />
/// <reference types="astro/client" />

// As per https://docs.astro.build/en/guides/environment-variables/#intellisense-for-typescript
interface ImportMetaEnv {
    readonly VERSION: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}