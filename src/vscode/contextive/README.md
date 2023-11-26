# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech)

Contextive is a Visual Studio Code extension to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the term definitions in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

![Example of Contextive in action.](../../../docs/wiki/images/simple-auto-complete-demo.gif)

## Getting Started

Use the `Contextive: Initialize Definitions` command from the command palette to create a sample definitions file. A file will be created and opened with a sample set of definitions:

![Example of a Contextive definition hover over the word "context" in a yml file.](../../../docs/wiki/images/example_hover.png)

This sample file illustrates the use of Contextive by defining the terms used in the definitions file yml structure itself.  You can hover over the name of the terms in the file to see Contextive in action (see the sample image above).

You should delete the sample definitions and replace them with your own.

## Usage Guide

See our [usage guide](../../../docs/wiki/USAGE.md) for details on the definitions file format and available options. 

## Features

* Initialize your Contextive Definitions
* [Auto-complete](../../../docs/wiki/USAGE.md#smart-auto-complete) from your Contextive Definitions
  * Shows definitions in auto-complete details
* Hover to show definitions from your Contextive Definitions
  * Hover over elements with [suffixes & prefixes](../../../docs/wiki/USAGE.md#suffixes-and-prefixes)
  * Hover over usage of [multiple terms](../../../docs/wiki/USAGE.md#combining-two-or-more-terms) combined using camelCase, PascalCase and snake_case
  * Hover over [multi-word](../../../docs/wiki/USAGE.md#multi-word-terms) terms
  * Hover over [plural](../../../docs/wiki/USAGE.md#plural-words) of defined terms
  * Hover over [aliases](../../../docs/wiki/USAGE.md#term-aliases) of defined terms
* Supported Repository Layouts:
  * A [repository per context](../../../docs/wiki/USAGE.md#multiple-bounded-contexts-repository-per-context)
  * [Multiple contexts in the same repository](../../../docs/wiki/USAGE.md#multiple-bounded-contexts-single-repository-single-root-monorepo) (monorepo) (identified by path globs)
  * Context distributed over [multiple repositories](../../../docs/wiki/USAGE.md#single-bounded-context-multiple-repositories) (#36)
  * [Multi-root workspaces](../../../docs/wiki/USAGE.md#multiple-bounded-contexts-multi-root-shared-definitions-file)
* Works in all files (uses the `*` document selector)

### Coming Soon

* UI to edit/manage Contextive Definitions
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Extension Settings

This extension contributes the following settings:

* `contextive.path`: The path of the file that stores the Contextive definitions.  Default: `.contextive/definitions.yml`

## Known Issues

* The extension only activates on the presence of the `.contextive` folder in the workspace.  If the `contextive.path` setting has been updated, the `.contextive` folder may not exist.  (The extension will also activate on use of the `Contextive: Initialize Definitions` command.)