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
		console.log('rawIpcOpts: ', rawIpcOpts);
		const ipcOpts = JSON.parse(rawIpcOpts);
		console.log('ipcOpts: ', ipcOpts);
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

const workspacePaths = {
	'Single-Root': 'single-root/fixtures/simple_workspace',
	'Multi-Root': 'multi-root/fixtures/multi-root_mono_workspace/multi-root.code-workspace',
}

function getLaunchArgs() {
	const mochaFGrep = process.env['MOCHA_FGREP'] || 'Single-Root';
	return [
		path.resolve(__dirname, `../test/${workspacePaths[mochaFGrep]}`),
		'--disable-gpu',
		'--disable-extension', 'ms-dotnettools.dotnet-interactive-vscode',
		'--disable-features=dbus'
	];
}

async function downloadVsCodeAndExtensions(version) {
	const vscodeExecutablePath = await downloadAndUnzipVSCode({version});
	const [cli, ...args] = resolveCliArgsFromVSCodeExecutablePath(vscodeExecutablePath, {
		version
	});
	const cwd = path.dirname(cli);
	const cmd = path.basename(cli);
	console.log('cli, cmd, cwd, args', cli, cmd, cwd, args);
	console.log("-------------------");
	console.log("Pre-install extensions...");
	cp.spawnSync(cmd, [...args, '--install-extension', 'ms-dotnettools.csharp'], {
		encoding: 'utf-8',
		stdio: 'inherit',
		shell: true,
		cwd,
	});
	console.log("Post-install extensions.")
	console.log("-------------------");
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

		await downloadVsCodeAndExtensions(version);

		const launchArgs = getLaunchArgs();
		const ipcOpts = getIpcEnv();

		console.log("Running tests...");

		// Download VS Code, unzip it and run the integration test
		await runTests({ 
			version,
			extensionDevelopmentPath,
			extensionTestsPath,
			launchArgs,
			extensionTestsEnv: {
				...ipcOpts,
			}
		});
	} catch (err) {
		console.error('Failed to run tests', err);
		process.exit(1);
	}
}

main();
