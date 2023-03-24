const path = require('path');
const cp = require('node:child_process');

const { 
	runTests,
	downloadAndUnzipVSCode,
	resolveCliArgsFromVSCodeExecutablePath,
} = require('@vscode/test-electron');

async function main() {
	try {
		// The folder containing the Extension Manifest package.json
		// Passed to `--extensionDevelopmentPath`
		const extensionDevelopmentPath = path.resolve(__dirname, '../');

		// The path to the extension test script
		// Passed to --extensionTestsPath
		const extensionTestsPath = path.resolve(__dirname, './index');

		const vscodeExecutablePath = await downloadAndUnzipVSCode();
		const [cli, ...args] = resolveCliArgsFromVSCodeExecutablePath(vscodeExecutablePath);
		cp.spawnSync(cli, [...args, '--install-extension', 'ms-dotnettools.csharp'], {
			encoding: 'utf-8',
			stdio: 'inherit',
		});

		const launchArgs = [
			path.resolve(__dirname, "../test/fixtures/simple_workspace"),
		];

		// Download VS Code, unzip it and run the integration test
		await runTests({ extensionDevelopmentPath, extensionTestsPath, launchArgs });
	} catch (err) {
		console.error('Failed to run tests', err);
		process.exit(1);
	}
}

main();
