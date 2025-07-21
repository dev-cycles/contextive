# Contextive README

[Documentation](https://docs.contextive.tech/community/v/1.16.0/) | [Releases](https://github.com/dev-cycles/contextive/releases) | [Subscribe for Updates](https://buttondown.com/contextive)

Contextive is an IntelliJ Platform Plugin to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the glossary in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

> [!WARNING]  
> This plugin is considered 'beta' status, as leverages relatively new [Language Server Protocol](https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html) support from JetBrains which is still marked as unstable in the IntelliJ Platform API. Some features may not work or may not work as expected. Please [report issues](https://github.com/dev-cycles/contextive/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=) in this project and we will liaise with JetBrains to resolve.
>
> See [known issues](#known-issues) below

## Installation

See [IntelliJ IDEs (e.g. IDEA) Installation Instructions](https://docs.contextive.tech/community/v/1.12.1/guides/installation/#intellij-plugin-platform).

## Getting Started

See [setting up glossaries](https://docs.contextive.tech/community/v/1.14.1/guides/setting-up-glossaries/) and [defining terminology](https://docs.contextive.tech/community/v/1.14.1/guides/defining-terminology/) usage guides for details on getting started with Contextive.

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

## Features

* [Auto-complete](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#smart-auto-complete) from your Contextive Glossary
  * Shows definitions in auto-complete details
    * Note: Press `F1` while the auto-complete list is shown to see the definition, and choose `Show Automatically During Completion` from the documentation panel's '3-dots' menu to have it show every time.)
* Hover to show definitions from your Contextive Glossary
  * Hover over elements with [suffixes & prefixes](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#suffixes-and-prefixes)
  * Hover over usage of [multiple terms](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#combining-two-or-more-terms) combined using camelCase, PascalCase and snake_case
  * Hover over [multi-word](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#complex-multi-word-terms) terms
  * Hover over [plural](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#plurals) of defined terms
  * Hover over [aliases](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#aliases) of defined terms
* Put your glossaries near the code they support:
  * [Terms relevant for the whole repository](https://docs.contextive.tech/community/v/1.14.1/guides/setting-up-glossaries/#terms-relevant-for-the-whole-repository)
  * [Different terms relevant in different repositories](https://docs.contextive.tech/community/v/1.14.1/guides/setting-up-glossaries/#different-terms-relevant-in-different-repositories)
  * [Terms relevant only in a subfolder of the repository](https://docs.contextive.tech/community/v/1.14.1/guides/setting-up-glossaries/#terms-relevant-only-in-a-subfolder-of-the-repository)
  * [Multi-root workspaces](https://docs.contextive.tech/community/v/1.14.1/guides/setting-up-glossaries/#multi-root-workspaces)
* Works in all files

### Coming Soon

* IDE command to initialize the glossary file
* Configure different location for the glossary file
* UI to edit/manage Contextive Glossary
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Plugin Configuration

The plugin does not currently support configuration.  The glossary file _must_ be in `./contextive/definitions.yml` in your current project folder.

This is pending IntelliJ support for the LSP `workspace/configuration` feature.

## Known Issues

* Not configurable - see [Plugin Configuration](#plugin-configuration)
* Autocomplete list doesn't respect the case sensitive interpretation as described in the [smart auto-complete](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/#smart-auto-complete) part of the [defining terminology](https://docs.contextive.tech/community/v/1.12.1/guides/defining-terminology/) usage guide. IntelliJ does not yet respect the `isIncomplete' flag that forces re-computation of the list and is required to adjust the case of list items.  See [IDEA-348829](https://youtrack.jetbrains.com/issue/IDEA-348829) for details. 
* Documentation not shown on hover in some files (files treated as 'plaintext' by the specific IDE). Use F1 to show the hover documentation at the current cursor position.  This is an IntelliJ bug, see [IDEA-348497](https://youtrack.jetbrains.com/issue/IDEA-348497/Doc-popup-doesnt-appear-on-hover-in-LSP-API-based-plugins) for details.
* Auto-completion does not work at certain positions. See [Issue #63](https://github.com/dev-cycles/contextive/issues/63)
* In Rider, C# files do not show documentation on hover, or 'quick documentation shortcut', or documentation during auto-complete, when opened in Solution mode.  It does work when opening in file/folder mode. See [Issue #65](https://github.com/dev-cycles/contextive/issues/65)