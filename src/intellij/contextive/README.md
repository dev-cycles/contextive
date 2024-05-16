# Contextive README

Contextive is an IntelliJ Platform Plugin to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the term definitions in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

> [!WARNING]  
> This plugin is considered 'beta' status, as leverages relatively new [Language Server Protocol](https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html) support from JetBrains which is still marked as unstable in the IntelliJ Platform API. Some features may not work or may not work as expected. Please [report issues](https://github.com/dev-cycles/contextive/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=) in this project and we will liaise with JetBrains to resolve.
>
> See [known issues](#known-issues) below

## Installation

See [IntelliJ IDEs (e.g. IDEA) Installation Instructions](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/INSTALLATION.md#intellij-plugin-platform).

## Getting Started

Create a folder in your project root called `.contextive`.  Create a file in that folder called `definitions.yml`.

Start defining your definitions following the schema specified in our [usage guide](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md).  You might like to start by copying our [default definitions](https://github.com/dev-cycles/contextive/blob/main/src/language-server/Contextive.LanguageServer.Tests/DefinitionsInitializationTests.Default%20Definitions.verified.txt) file that defines the terms used in the definitions file itself.

## Supported IDEs

The plugin uses the IntelliJ Language Server Protocol support, so it's only available in the IDEs where that feature is offered.  See the [JetBrains LSP Documentation](https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html#supported-ides) for the latest list.

At time of writing, it includes:

* IntelliJ IDEA Ultimate
* WebStorm
* PhpStorm
* PyCharm Professional
* DataSpell
* RubyMine
* CLion
* Aqua
* DataGrip
* GoLand
* Rider - [not working in C# files](https://github.com/dev-cycles/contextive/issues/65)
* RustRover

## Usage Guide

See our [usage guide](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md) for details on the definitions file format and available options. 

## Features

* [Auto-complete](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#smart-auto-complete) from your Contextive Definitions
  * Shows definitions in auto-complete details
    * Note: Press `F1` while the auto-complete list is shown to see the definition, and choose `Show Automatically During Completion` from the documentation panel's '3-dots' menu to have it show every time.)
* Hover to show definitions from your Contextive Definitions
  * Hover over elements with [suffixes & prefixes](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#suffixes-and-prefixes)
  * Hover over usage of [multiple terms](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#combining-two-or-more-terms) combined using camelCase, PascalCase and snake_case
  * Hover over [multi-word](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#multi-word-terms) terms
  * Hover over [plural](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#plural-words) of defined terms
  * Hover over [aliases](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#term-aliases) of defined terms
* Supported Repository Layouts:
  * A [repository per context](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#multiple-bounded-contexts-repository-per-context)
  * [Multiple contexts in the same repository](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#multiple-bounded-contexts-single-repository-single-root-monorepo) (monorepo) (identified by path globs)
  * Context distributed over [multiple repositories](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#single-bounded-context-multiple-repositories) (#36)
  * [Multi-root workspaces](https://github.com/dev-cycles/contextive/blob/main/docs/wiki/USAGE.md#multiple-bounded-contexts-multi-root-shared-definitions-file)
* Works in all files

### Coming Soon

* IDE command to initialize the definitions file
* Configure different location for the definitions file
* UI to edit/manage Contextive Definitions
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Plugin Configuration

The plugin does not currently support configuration.  The definitions file _must_ be in `./contextive/definitions.yml` in your current project folder.

This is pending IntelliJ support for the LSP `workspace/configuration` feature.

## Known Issues

* Not configurable - see [Plugin Configuration](#plugin-configuration)
* Autocomplete list doesn't respect the case sensitive interpretation as described in the usage guide. IntelliJ does not yet respect the `isIncomplete' flag that forces re-computation of the list and is required to adjust the case of list items.  See [IDEA-348829](https://youtrack.jetbrains.com/issue/IDEA-348829) for details. 
* Documentation not shown on hover in some files (files treated as 'plaintext' by the specific IDE). Use F1 to show the hover documentation at the current cursor position.  This is an IntelliJ bug, see [IDEA-348497](https://youtrack.jetbrains.com/issue/IDEA-348497/Doc-popup-doesnt-appear-on-hover-in-LSP-API-based-plugins) for details.
* Auto-completion does not work at certain positions. See [Issue #63](https://github.com/dev-cycles/contextive/issues/63)
* In Rider, C# files do not show documentation on hover, or 'quick documentation shortcut', or documentation during auto-complete, when opened in Solution mode.  It does work when opening in file/folder mode. See [Issue #65](https://github.com/dev-cycles/contextive/issues/65)