const path = require('path');
const Mocha = require('mocha');
const { glob } = require('glob');

function getMochaOpts() {
	const defaultOpts = {timeout: 60000};
	if (process.env.MOCHA_FGREP) {
		return {
			fgrep: process.env.MOCHA_FGREP,
			...defaultOpts
		};
	}
	return defaultOpts;
}

function run() {
	const testsRoot = __dirname;

	const opts = getMochaOpts();
	// Create the mocha test

	const resultsPath = path.resolve(testsRoot, `../TestResults/TestResults-${opts.fgrep}-${process.env.DOTNET_VERSION}-${process.env.RUNNER_OS}.xml`);
	console.log (`Test Results Path: ${resultsPath}`);

	const mocha = new Mocha(opts)
	.reporter('mocha-multi-reporters', {
		reporterEnabled: "list, mocha-junit-reporter",
		mochaJunitReporterReporterOptions: {
			mochaFile: resultsPath
		}
	});


	return new Promise((c, e) => {
		glob('**/**.test.js', { cwd: testsRoot }).then((files) => {
			// Add files to the test suite
			files.forEach(f => mocha.addFile(path.resolve(testsRoot, f)));

			try {
				// Run the mocha test
				mocha.run(failures => {
					if (failures > 0) {
						e(new Error(`${failures} tests failed.`));
					} else {
						c();
					}
				});
			} catch (err) {
				console.error(err);
				e(err);
			}
		});
	});
}

module.exports = {
	run
};
