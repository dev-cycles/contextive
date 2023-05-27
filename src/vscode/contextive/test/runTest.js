const path = require('path');
const cp = require('node:child_process');

const { 
	runTests,
	downloadAndUnzipVSCode,
	resolveCliArgsFromVSCodeExecutablePath,
} = require('@vscode/test-electron');

function getIpcEnv() {
	if (process.argv[2]) {
		const rawIpcOpts = process.argv[2];
		console.log('ipcOpts: ', rawIpcOpts);
		const ipcOpts = JSON.parse(rawIpcOpts);
		return {
			MOCHA_WORKER_IPC_ROLE: ipcOpts.role,
			MOCHA_WORKER_IPC_PORT: String(ipcOpts.port),
			MOCHA_WORKER_IPC_HOST: ipcOpts.host
		}
	}
	return {};
}

function getExtensionTestsPath() {
	const mochaWorkerPath = process.env['MOCHA_WORKER_PATH'];
	if (mochaWorkerPath) {
		return path.resolve(__dirname, '../node_modules/mocha-explorer-launcher-scripts/vscode-test/runMochaWorker');
	}
	return path.resolve(__dirname, './index');
}

async function main() {
	try {
		// The folder containing the Extension Manifest package.json
		// Passed to `--extensionDevelopmentPath`
		const extensionDevelopmentPath = path.resolve(__dirname, '../');

		const version = process.env['VSCODE_VERSION'];

		console.log(`Version: ${version}`);

		// The path to the extension test script
		// Passed to --extensionTestsPath
		const extensionTestsPath = getExtensionTestsPath();
		console.log(`Extension Test Path: ${extensionTestsPath}`);

		const vscodeExecutablePath = await downloadAndUnzipVSCode({version});
		const [cli, ...args] = resolveCliArgsFromVSCodeExecutablePath(vscodeExecutablePath, {
			version
		});
		console.log("cli, args", cli, args);
		cp.spawnSync(cli, [...args, '--install-extension', 'ms-dotnettools.csharp'], {
			encoding: 'utf-8',
			stdio: 'inherit',
		});

		const launchArgs = [
			path.resolve(__dirname, "../test/fixtures/simple_workspace"),
		];
		const ipcOpts = getIpcEnv();
		console.log(ipcOpts);

		// Download VS Code, unzip it and run the integration test
		await runTests({ 
			version,
			extensionDevelopmentPath,
			extensionTestsPath,
			launchArgs,
			extensionTestsEnv: {
				CONTEXTIVE_DEBUG: true,
				...ipcOpts,
			}
		});
	} catch (err) {
		console.error('Failed to run tests', err);
		process.exit(1);
	}
}

main();
