# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml) [![Github](https://img.shields.io/github/stars/dev-cycles/contextive
)](https://github.com/dev-cycles/contextive) [![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?logo=bluesky&logoColor=fff)](https://bsky.app/profile/contextive.tech) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![LinkedIn](https://custom-icon-badges.demolab.com/badge/LinkedIn-0A66C2?logo=linkedin-white&logoColor=fff)](https://www.linkedin.com/company/contextive-tech)

Contextive is a Visual Studio Code extension to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the glossary in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

![Example of Contextive in action.](../../../docs/web/src/assets/images/simple-auto-complete-demo.gif)

## Installation

See [VsCode Installation Instructions](https://docs.contextive.tech/ide/v/1.14.1/guides/installation/#visual-studio-code).

## Getting Started

Use the `Contextive: Initialize Glossary File` command from the command palette to create a sample glossary file. A file will be created and opened with a sample set of definitions:

![Example of a Contextive definition hover over the word "context" in a yml file.](../../../docs/web/src/assets/images/example_hover.png)

This sample file illustrates the use of Contextive by defining the terms used in the glossary file yml structure itself.  You can hover over the name of the terms in the file to see Contextive in action (see the sample image above).

You should delete the sample definitions and replace them with your own.

## Usage Guide

See our [usage guide](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/) for details on the glossary file format and available options. 

## Features

* Initialize your Contextive Glossary
* [Auto-complete](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#smart-auto-complete) from your Contextive Glossary
  * Shows definitions in auto-complete details
* Hover to show definitions from your Contextive Glossary
  * Hover over elements with [suffixes & prefixes](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#suffixes-and-prefixes)
  * Hover over usage of [multiple terms](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#combining-two-or-more-terms) combined using camelCase, PascalCase and snake_case
  * Hover over [multi-word](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#multi-word-terms) terms
  * Hover over [plural](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#plural-words) of defined terms
  * Hover over [aliases](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#term-aliases) of defined terms
* Supported Repository Layouts:
  * A [repository per context](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#multiple-bounded-contexts-repository-per-context)
  * [Multiple contexts in the same repository](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#multiple-bounded-contexts-single-repository-single-root-monorepo) (monorepo) (identified by path globs)
  * Context distributed over [multiple repositories](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#single-bounded-context-multiple-repositories) (#36)
  * [Multi-root workspaces](https://docs.contextive.tech/ide/v/1.14.1/guides/usage/#multiple-bounded-contexts-multi-root-shared-definitions-file)
* Works in all files (uses the `*` document selector)

### Coming Soon

* UI to edit/manage Contextive Glossary
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Extension Settings

This extension contributes the following settings:

* `contextive.path`: The path of the file that stores the Contextive glossary.  Default: `.contextive/definitions.yml`

## Known Issues

* The extension only activates on the presence of the `.contextive` folder in the workspace.  If the `contextive.path` setting has been updated, the `.contextive` folder may not exist.  (The extension will also activate on use of the `Contextive: Initialize Glossary File` command.)