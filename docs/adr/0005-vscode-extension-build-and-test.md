# VsCode Extension Build and Test 

* Status: accepted
* Deciders: Chris Simon
* Date: 2021-11-05

## Decision Summary

Use [Fable](https://fable.io/) and then [webpack](https://webpack.js.org/) to ensure VsCode host compliant javascript.

Use [Fable.Mocha](https://github.com/Zaid-Ajaj/Fable.Mocha) as the test library.

Use separate webpack config files for build and test to ensure the entry point is appropriate for the VsCode extension test expectations.

## Context and Problem Statement

In ADR0002 it was determined to use F# and Fable to write the VsCode extension.

While Fable supports general transpilation of F# to javaScript, there are a few specific factors involved in compiling a JavaScript codebase that will work as a VsCode Extension:

1. VsCode extension host [does not support ES6 modules](https://github.com/microsoft/vscode/issues/116056) (which is the default module type produced by Fable v3)
1. VsCode extension integration testing has [specific requirements for how to run the tests](https://code.visualstudio.com/api/working-with-extensions/testing-extension).

In addition, Fable has a number of options for how to [build and test](https://fable.io/docs/your-fable-project/build-and-run.html) a JavaScript artefact, and the recommended approach has changed significantly over the last few years.

This ADR captures the decision around how to build and test F# with Fable to meet the above constraints.

## Decision Drivers

The selected build and test approach must:

* Allow execution as a VsCode Extension
* Allow running VsCode Extension tests

## Considered Options

* Fable alone
* Fable with Babel
* Fable with Webpack

## Decision Outcome

Fable with webpack was the selected option.  The general extension architecture is modelled on the TypeScript sample extension, as transpilation from TypeScript to JavaScript necessitates a similar structure as transpilation from F# to JavaScript.

The `src` folder contains an F# project - `Ubictionary.VsCodeExtension.fsproj` which can be independently compiled by Fable.

The `test` folder contains a separate F# project - `Ubictionary.vSCodeExtension.Tests.fsproj` which can be independently compiled by Fable and holds a dotnet project reference to `Ubictionary.VsCodeExtension.fsproj`.

Tests are written using `Fable.Mocha` due to support for similar structures/patterns to the [Expecto](https://github.com/haf/expecto) library being used in the Language Server tests. (Note: no Fable equivalent of the [Unquote](https://github.com/SwensenSoftware/unquote) library has yet been found, so the extension tests use the default Fable.Mocha assertion patterns, which are modelled on Expecto.)

### Compilation for Execution/Debug

Compilation for execution proceeds in two steps:

1. Fable compilation of the `Ubictionary.VsCodeExtension.fsproj` project as an 'in-place' compilation producing `.js` files beside the `.fs` files
2. Webpack with output to the `dist` folder, and packing to a single output file - `./dist/extension.js`.  The extension entry point (`main` in `package.json`) is set to `./dist/extension.js`.

### Compilation for Testing

Compilation for test also proceeds in two steps:

1. Fable compilation of the `Ubictionary.VsCodeExtension.Tests.fsproj` project as an `in-place` compilation producing `.js` files beside the `.fs` files.  Fable will follow the project reference and ensure `Ubictionary.VsCodeExtension.fsproj` is built in a similar way.
2. Webpack with output to the `out` folder. A custom webpack configuration file for testing is used.  It has 3 entry points to ensure that individual javascript files required to support the VsCode extension testing mechanisms are transpiled individually, resulting in three files in the `out` folder:

* `main.test.js` - the transpiled & packed test and extension code. This pulls in all the referenced files from the actual extension project so relative module imports are no longer needed.
* `index.js` - a `run` export that will execute the Mocha tests (modelled on the VsCode samples) - used by the VsCode extension debugger to launch in an Extension Development Host
* `runTests.js` - a node.js script (modelled on the VsCode samples) that will be run in CI.  It uses the `@vscode/test-electron` library to download a copy of VsCode and launch the Extension Development Host from a shell process targetting the `index.js` mocha test runner.

### Positive Consequences

* It works!

### Negative Consequences

* It's a bit unusual - it looks a bit like the methods used by the VsCode TypeScript extensions, but isn't quite the same.  It also looks a bit like the methods used by the Fable web application samples, but isn't quite the same. Details below.

## Pros and Cons of the Options

### Fable Alone

Using standard Fable compilation techniques, including the use of a destination folder to compile to the `dist` output folder.

* Good, because it's simple
* Good, because it's very similar to TypeScript extension structure, except using the Fable compile instead of the TypeScript complier
* Bad, because it produces ES6 modules which can't be imported in the VsCode extension host Node.js runtime.

### Fable with Babel

Using standard Fable compilation techniques, then running through Babel to transpile to ES5 and CommonJs module format.

* Bad, because relative imports aren't translated properly, particularly for tests.

### Fable with Webpack

Fable with webpack post processing.

* Good, because it works and meets all constraints
* Bad, because it's a bit unusual.  Specifically:
  * When using TypeScript with webpack, typically webpack is the only step and the `ts-loader` is used to pull in and compile the TypeScript files. While Fable used to support this method using a similar `fs-loader` it's now recommended to just use Fable and then post-process with webpack.
  * When using Fable with webpack, it's not common to require a separate webpack configuration for testing - the need for separate `index.js` and `runTests.js` (as described above) is unique to VsCode Extension development.
    * Future work may discover a way of refactoring this configuration towards a more standard pattern.