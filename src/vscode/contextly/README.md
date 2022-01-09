# Contextly README

Contextly is a Visual Studio Code extension to promote the use of a Domain Driven Design Ubiquitous Language on your projects.  It should help new team members get up to speed more quickly in understanding domain-specific terms.

By storing the term definitions in your repository, it encourages regularly updating and evolving the definitions as the domain evolves and team's understanding improves, in line with the implementation of the code.

## Getting Started

Use the `Contextly: Initialize Definitions` command from the command palette to create a sample definitions file.  This sample file defines the terms used in the definitions file yml structure.  You can hover over the name of the terms in the file to see Contextly in action.

## Features

* [x] Initialize your Contextly Definitions
* [x] Auto-complete from your Contextly Definitions
* [x] Hover to show definitions from your Contextly Definitions

#### Coming Soon

* [ ] UI to edit/manage Contextly Definitions
* [ ] Support for multiple contexts in the same or separate repos

## Extension Settings

This extension contributes the following settings:

* `contextly.path`: The path of the `contextly.yml` file that stores the Contextly definitions.  Default: `.contextly/definitions.yml`