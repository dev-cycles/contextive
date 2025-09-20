# Contextive README

[Documentation](https://docs.contextive.tech/community/v/1.16.0/) | [Releases](https://github.com/dev-cycles/contextive/releases) | [Subscribe for Updates](https://buttondown.com/contextive)

Contextive is an IntelliJ Platform Plugin to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the glossary in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

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

## IntelliJ Platform Compatibility

Contextive relies on the [IntelliJ Platform Language Server Protocol APIs](https://plugins.jetbrains.com/docs/intellij/language-server-protocol.html) which were marked as experimental until IntelliJ platform 2025.1.  It's recommended to use 2025.1 or later.

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
* UI to edit/manage Contextive Glossary
* Internationalization support
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Known Issues

* Auto-completion does not work at certain positions. See [Issue #63](https://github.com/dev-cycles/contextive/issues/63)
* In Rider, C# files do not show documentation on hover, or 'quick documentation shortcut', or documentation during auto-complete, when opened in Solution mode.  It does work when opening in file/folder mode. See [Issue #65](https://github.com/dev-cycles/contextive/issues/65)
* When Contextive is installed, sometimes the 'native' language documentation is not shown.  See [Issue #89](https://github.com/dev-cycles/contextive/issues/89) and [IJPL-181566](https://youtrack.jetbrains.com/issue/IJPL-181566/LSP-backed-hover-info-should-co-exist-with-native-quick-docs-it-shouldnt-replace-them)