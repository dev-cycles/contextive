const path = require('path');
const Mocha = require('mocha');
const glob = require('glob');

function run() {
	const testsRoot = __dirname;

	// Create the mocha test
	const mocha = new Mocha({
		ui: 'bdd', // Fable.Mocha uses `describe`, so we need to use the `bdd` API. See https://mochajs.org/#interfaces
		color: true
	})
	.reporter('mocha-multi-reporters', {
		reporterEnabled: "spec, mocha-junit-reporter",
		mochaJunitReporterReporterOptions: {
			mochaFile: path.resolve(testsRoot, `../TestResults/TestResults-${process.env.DOTNET_VERSION}.xml`)
		}
	});

	return new Promise((c, e) => {
		glob('**/**.test.js', { cwd: testsRoot }, (err, files) => {
			if (err) {
				return e(err);
			}

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
