# Contextly README

Contextly is a Visual Studio Code extension to promote the use of a Domain Driven Design Ubiquitous Language on your projects.  It should help new team members get up to speed more quickly in understanding domain-specific terms.

By storing the term definitions in your repository, it encourages regularly updating and evolving the definitions as the domain evolves and team's understanding improves, in line with the implementation of the code.

## Getting Started

Use the `Contextly: Initialize Definitions` command from the command palette to create a sample definitions file.  This sample file defines the terms used in the definitions file yml structure.  You can hover over the name of the terms in the file to see Contextly in action.

## Features

* Initialize your Contextly Definitions
* Auto-complete from your Contextly Definitions
* Hover to show definitions from your Contextly Definitions
* Currently configured to work in files of type: c, cpp, csharp, fsharp, go, groovy, html, java, javascript, javascriptreact, json, jsonc, markdown, perl, php, plaintext, powershell, python, ruby, rust, sql, typescript, typescriptreact, vb, xml, yaml

#### Coming Soon

* UI to edit/manage Contextly Definitions
* Show definitions in auto-complete details
* Internationalization support
* Support for multiple contexts in the same or separate repos
* Configurable list of language identifiers. The list is currently hard coded as above.
* Better support for key word identification in different languages
* Support for word identification in combined usage such as camelCase, PascalCase and snake_case
* Support for documenting combined words (e.g. verbNoun or noun_verbed)

## Extension Settings

This extension contributes the following settings:

* `contextly.path`: The path of the `contextly.yml` file that stores the Contextly definitions.  Default: `.contextly/definitions.yml`

## Known Issues

* The extension only activates on the presence of the `.contextly` folder in the workspace.  If the `contextly.path` setting has been updated, the `.contextly` folder may not exist.  (The extension will also activate on use of the `Contextly: Initialize Definitions` command.)