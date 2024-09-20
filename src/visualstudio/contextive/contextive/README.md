# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech)

Contextive is a Visual Studio Code extension to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the term definitions in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

> [!WARNING]  
> This plugin is considered 'beta' status, as leverages relatively new [Language Server Protocol](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/language-server-provider/language-server-provider?view=vs-2022) support in the [Preview Extensibility Model](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/?view=vs-2022). Some features may not work or may not work as expected. Please [report issues](https://github.com/dev-cycles/contextive/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=) in this project and we will liaise with Microsoft & the Visual Studio team to resolve.
> 
> The main issue for most use cases is that the extension doesn't work when opening a solution file, only when opening a folder.  See [Issue #75](https://github.com/dev-cycles/contextive/issues/75) for details.
>
> See [known issues](https://github.com/dev-cycles/contextive/blob/v1.12.1/src/visualstudio/contextive/contextive/README.md#known-issues) for others.

## Installation

See [Visual Studio Instructions](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/INSTALLATION.md#visual-studio-2022).

## Getting Started

Create a folder in your project root called `.contextive`.  Create a file in that folder called `definitions.yml`.

Start defining your definitions following the schema specified in our [usage guide](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md). You might like to start by copying our [default definitions](https://github.com/dev-cycles/contextive/blob/v1.12.1/src/language-server/Contextive.LanguageServer.Tests/DefinitionsInitializationTests.Default%20Definitions.verified.txt) file that defines the terms used in the definitions file itself.

## Usage Guide

See our [usage guide](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md) for details on the definitions file format and available options. 

## Features

* Initialize your Contextive Definitions
* [Auto-complete](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#smart-auto-complete) from your Contextive Definitions
  * Shows definitions in auto-complete details
* Hover to show definitions from your Contextive Definitions
  * Hover over elements with [suffixes & prefixes](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#suffixes-and-prefixes)
  * Hover over usage of [multiple terms](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#combining-two-or-more-terms) combined using camelCase, PascalCase and snake_case
  * Hover over [multi-word](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#multi-word-terms) terms
  * Hover over [plural](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#plural-words) of defined terms
  * Hover over [aliases](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#term-aliases) of defined terms
* Supported Repository Layouts:
  * A [repository per context](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#multiple-bounded-contexts-repository-per-context)
  * [Multiple contexts in the same repository](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#multiple-bounded-contexts-single-repository-single-root-monorepo) (monorepo) (identified by path globs)
  * Context distributed over [multiple repositories](https://github.com/dev-cycles/contextive/blob/v1.12.1/docs/wiki/USAGE.md#single-bounded-context-multiple-repositories) ([#36](https://github.com/dev-cycles/contextive/issues/36))

### Coming Soon

* UI to edit/manage Contextive Definitions
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Extension Settings

The extension does not currently support configuration.  The definitions file _must_ be in `./contextive/definitions.yml` in your open folder.

## Known Issues

* [Doesn't work when opening a Solution, only when opening a folder](https://github.com/dev-cycles/contextive/issues/75)
* [Definitions don't update in hover panels when the definitions file is updated](https://github.com/dev-cycles/contextive/issues/79)
* [Doesn't work in all file types](https://github.com/dev-cycles/contextive/issues/78)
* [Markdown not supported in hover panel](https://github.com/dev-cycles/contextive/issues/76)

See [All Contextive Visual Studio Issues](https://github.com/dev-cycles/contextive/issues?q=is%3Aissue+is%3Aopen+label%3AVisualStudio).