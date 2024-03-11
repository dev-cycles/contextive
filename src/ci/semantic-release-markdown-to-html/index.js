import { prepare as replacePrepare } from 'semantic-release-replace-plugin';
import markDownIt from 'markdown-it';
import fsFull from 'fs';

const fs = fsFull.promises;
const md = markDownIt();

const prepare = async (pluginConfig, context) => {
    context.htmlFiles = {};
    for (var [key, markDownPath] of Object.entries(pluginConfig.files)) {
        context.logger.log(`Generating HTML for ${key} from ${markDownPath}`);
        const markdownContent = await fs.readFile(markDownPath, 'UTF-8');
        context.htmlFiles[key] = md.render(markdownContent);
    }
    replacePrepare(pluginConfig, context);
}

export { prepare };