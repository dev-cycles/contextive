# Contextive

![Contextive Banner](images/contextive_banner.png)

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive-vscode.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive-vscode.yml) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech)

Contextive is a suite of tools to immerse developers in the language of their users' domains.

## Inspiration

Contextive is inspired by the concept of the [Ubiquitous Language](https://martinfowler.com/bliki/UbiquitousLanguage.html) from the practice of [Domain Driven Design (DDD)](https://martinfowler.com/bliki/DomainDrivenDesign.html) and is intended to support ubiquitous language management practices on DDD projects.

Even if you're not using Domain Driven Design, Contextive should still be very helpful in any software project where it's important that developers are aligned on the meaning of terms.

By defining terms in a central definitions file, Contextive can surface definitions and usage examples in auto-complete suggestions & hover panels wherever the terms are used - in code (of any language across the stack), comments, config, and documentation (e.g. markdown).

![Example of Contextive in action.](src/vscode/contextive/images/simple-auto-complete-demo.gif)

## Proposed Features

The following list of features is a draft proposal of the vision at the start of the project.  It's expected that the list will evolve as a prototype is developed and experimented with and lessons are learnt.

* [ ] Ubiquitous Language Definition & Storage:
  * [ ] Classify terms as aggregates/entities, attributes, commands and events
  * [x] For each term, include a definition, and examples of usage in normal sentences
  * [ ] Link terms to each other (e.g. commands to the aggregates/entities they apply to; events to the aggregates/entities that publish them)
  * [x] Store Ubiquitous Language term definitions in a file in the repository (e.g. yml format)
  * [x] Support a monorepo with multiple Bounded Contexts in one repo - identify which paths relate to each Bounded Context
  * [x] Support a Bounded Context distributed across multiple repos (#36)
  * [x] Support aliases of terms, hovering over the alias shows the term definition
    * [x] Add details of the alias in the hover
    * [ ] Add ability to define an alias as deprecated and warn as such
* [ ] IDE Support
  * [x] Visual Studio Code
    * [x] Support single-root workspaces
    * [x] Support multi-root workspaces with a shared definitions file
    * [ ] Support multi-root workspaces with a definitions file per root
  * [ ] Visual Studio (2019/2022)
  * [ ] Eclipse
  * [ ] NetBeans
  * [ ] JetBrains
  * [ ] vim
  * [ ] emacs
* [ ] Code-editing Features
  * [x] Show the term definitions & usage examples when hovering over the word in the editor 
    * [x] Also when the word being hovered over is plural of the defined singular term
    * [ ] Also when the word being hovered over is singular of the defined plural term
    * [ ] Also when the combined word being hovered over is contains singular or plural of a word in a defined multi-word term
  * [x] Add Ubiquitous Language terms to the auto-complete list
  * [ ] Codelens to identify the number of Ubiquitous Language terms in a method or class
  * [ ] Problems/warnings if misuse of the Ubiquitous Language - e.g. use of Command/Event and aggregate in the same class or method name when they are not linked, or use of extra words next to an Aggregate that _isn't_ defined
  * [ ] In relevant places (e.g. hover), note the use of the term in other contexts to highlight the contrast and ensure clarity of which definition applies in the current context (e.g. '_THIS_ definition, _NOT_ that other definition that you might have been thinking of')
* [ ] Ubiquitous Language Management Features
  * [ ] Go To term definition (right-click menu, keyboard shortcut)
  * [ ] A UI widget to view the language terms in a TreeView and offer facilities for adding, updating & removing terms
  * [ ] Make it easy to add terms to the Ubiquitous Language from existing code e.g. by highlighting and using the right-click menu
* [ ] Ubiquitous Language Sharing Features
  * [ ] Render definitions into a human readable format - e.g. html, markdown etc.
  * [ ] Sync definitions into a cloud storage, e.g. Notion database, or confluence page
  * [ ] Above features might be well packaged as a CLI as well as extension features, for running in CI/CD

## Installation

### Visual Studio Code

Open Visual Studio Code, launch the quick open (`Ctrl+P`) and then enter `ext install devcycles.contextive`.  OR, search `contextive` in the extensions side-bar.

Visit the [Contextive Marketplace](https://marketplace.visualstudio.com/items?itemName=devcycles.contextive) page for details.

Check the extension [README](src/vscode/contextive/README.md) for usage instructions.

### Others

Coming soon!

## Contributing

If you have ideas for features or implementation, please open an issue to record your thoughts.

PRs welcome, but as things are evolving rapidly, a conversation before any major changes is probably worthwhile.

Key architectural decisions are tracked using ADRs (Architectural Decision Records) which can be found in the [docs/adr](docs/adr) folder.  The [MADR](https://adr.github.io/madr/) format is the current default.

Development is being done using [Visual Studio Code Dev Containers](https://code.visualstudio.com/docs/remote/containers). After cloning the repository and opening in Visual Studio Code, choose the "Reopen in Container" option to work in the dev environment. See the Visual Studio Code documentation for more details on setting up your docker environment.

## Logo

The Contextive logo is based (with extreme gratitude!) on the `Bibliophile` image from https://undraw.co - an amazing resource of free images and illustrations.

## License

[MIT](https://choosealicense.com/licenses/mit/)