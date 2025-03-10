import { type StarlightPlugin } from "@astrojs/starlight/types";

export default function (_) {
    return {
        name: 'Version Banner',
        hooks: {
            setup: options => {

            },
        }
    } as StarlightPlugin;
};
