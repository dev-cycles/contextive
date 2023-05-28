//@ts-check

'use strict';

const path = require('path');
const nodeExternals = require('webpack-node-externals');

//@ts-check
/** @typedef {import('webpack').Configuration} WebpackConfig **/

/** @type WebpackConfig */
const extensionConfig = {
  target: 'node', // vscode extensions run in a Node.js-context 📖 -> https://webpack.js.org/configuration/node/
  mode: 'none', // this leaves the source code as close as possible to the original (when packaging we set this to 'production')

  entry: {
    "multi-root.main.test": './test/multi-root/Main.test.fs.js',
    "single-root.main.test": './test/single-root/Main.test.fs.js', // the entry point of this extension, 📖 -> https://webpack.js.org/configuration/entry-context/
    index: './test/index.js', // the Mocha test runner 📖 -> https://code.visualstudio.com/api/working-with-extensions/testing-extension
    runTest: './test/runTest.js' // the VSCode test runner that downloads and launches VsCode 📖 -> https://code.visualstudio.com/api/working-with-extensions/testing-extension
  },
  output: {
    // the bundle is stored in the 'out' folder (check package.json), 📖 -> https://webpack.js.org/configuration/output/
    path: path.resolve(__dirname, 'out'),
    filename: '[name].js',
    libraryTarget: 'commonjs2'
  },
  externals: [{
      vscode: 'commonjs vscode', // the vscode-module is created on-the-fly and must be excluded.
      // Add other modules that cannot be webpack'ed, 📖 -> https://webpack.js.org/configuration/externals/
      // modules added here also need to be added in the .vscodeignore file
    },
    nodeExternals()
  ],
  resolve: {
    // support reading JavaScript files
    extensions: ['.js'],
  },
  module: {

  },
  infrastructureLogging: {
    level: "log", // enables logging required for problem matchers
  },
};
module.exports = [ extensionConfig ];