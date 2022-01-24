# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml) [![Twitter](https://img.shields.io/twitter/url/https/twitter.com/contextive_tech.svg?style=social&label=Follow%20%40Contextive)](https://twitter.com/contextive_tech)

Contextive is a Visual Studio Code extension to promote the use of a Domain Driven Design Ubiquitous Language on your projects.  It should help new team members get up to speed more quickly in understanding domain-specific terms.

By storing the term definitions in your repository, it encourages regularly updating and evolving the definitions as the domain evolves and team's understanding improves, in line with the implementation of the code.

![Example of a Contextive definition hover over the word "context" in a yml file.](images/example_hover.png)

## Getting Started

Use the `Contextive: Initialize Definitions` command from the command palette to create a sample definitions file.  This sample file defines the terms used in the definitions file yml structure.  You can hover over the name of the terms in the file to see Contextive in action.

## Features

* Initialize your Contextive Definitions
* Auto-complete from your Contextive Definitions
* Hover to show definitions from your Contextive Definitions
* Currently configured to work in files of type: c, cpp, csharp, fsharp, go, groovy, html, java, javascript, javascriptreact, json, jsonc, markdown, perl, php, plaintext, powershell, python, ruby, rust, sql, typescript, typescriptreact, vb, xml, yaml

#### Coming Soon

* UI to edit/manage Contextive Definitions
* Show definitions in auto-complete details
* Internationalization support
* Support for multiple contexts in the same or separate repos
* Configurable list of language identifiers. The list is currently hard coded as above.
* Better support for key word identification in different languages (e.g. different syntax delimiters)
* Support for word identification in combined usage such as camelCase, PascalCase and snake_case
* Support for detecting plural or singular versions of terms
* Support for documenting combined words (e.g. verbNoun or noun_verbed)

## Extension Settings

This extension contributes the following settings:

* `contextive.path`: The path of the file that stores the Contextive definitions.  Default: `.contextive/definitions.yml`

## Known Issues

* The extension only activates on the presence of the `.contextive` folder in the workspace.  If the `contextive.path` setting has been updated, the `.contextive` folder may not exist.  (The extension will also activate on use of the `Contextive: Initialize Definitions` command.)