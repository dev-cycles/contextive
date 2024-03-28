# Contextive

![Contextive Banner](images/contextive_banner.png)

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech) 

Contextive is a suite of tools to immerse developers in the language of their users' domains.

## Inspiration

Contextive is inspired by the concept of the [Ubiquitous Language](https://martinfowler.com/bliki/UbiquitousLanguage.html) from the practice of [Domain Driven Design (DDD)](https://martinfowler.com/bliki/DomainDrivenDesign.html) and is intended to support ubiquitous language management practices on DDD projects.

Even if you're not using Domain Driven Design, Contextive should still be very helpful in any software project where it's important that developers are aligned on the meaning of terms.

By defining terms in a central definitions file, Contextive can surface definitions and usage examples in auto-complete suggestions & hover panels wherever the terms are used - in code (of any language across the stack), comments, config, and documentation (e.g. markdown).

![Example of Contextive in action.](docs/wiki/images/simple-auto-complete-demo.gif)


## Installation

See our [Installation Guide](./docs/wiki/INSTALLATION.md) for details on how to install in a few different IDEs, or any IDE that supports the Language Server Protocol.

Officially supported IDEs include:

#### [VsCode](./docs/wiki/INSTALLATION.md#visual-studio-code)
#### [IntelliJ IDEs (e.g. IDEA, except Rider)](./docs/wiki/INSTALLATION.md#intellij-plugin-platform)
#### [Neovim](./docs/wiki/INSTALLATION.md#neovim)
#### [Helix](./docs/wiki/INSTALLATION.md#helix)

## Configuration & Usage

See our [Usage Guide](./docs/wiki/USAGE.md) for details on configuring Contextive, setting up your definitions file, and the various features and options available to you in defining your domain-specific terminology.

## Features

The following list is a rough and evolving backlog/roadmap.  Checked items are completed, others are not a commitment, just ideas that have been suggested.

* [ ] Ubiquitous Language Definition & Storage:
  * [ ] Classify terms as aggregates/entities, attributes, commands and events
  * [x] For each term, include a definition, and examples of usage in normal sentences
  * [ ] Link terms to each other (e.g. commands to the aggregates/entities they apply to; events to the aggregates/entities that publish them)
  * [ ] Define relationships between contexts (e.g. a Context Map definition)
  * [x] Store Ubiquitous Language term definitions in a file in the repository (e.g. yml format)
  * [x] Repository Layouts:
    * [x] Support a monorepo with multiple Bounded Contexts in one repo - identify which paths relate to each Bounded Context
    * [x] Support a Bounded Context distributed across multiple repos (#36)
  * [x] Support aliases of terms, hovering over the alias shows the term definition
  * [x] Add details of the alias in the hover
  * [ ] Add ability to define an alias as deprecated and warn as such
  * [x] Support multiline domain vision statements, definitions and usage examples
* [ ] IDE Support
  * [x] [Visual Studio Code](#visual-studio-code)
    * [x] Support single-root workspaces
    * [x] Support multi-root workspaces with a shared definitions file
    * [ ] Support multi-root workspaces with a definitions file per root
  * [ ] Visual Studio (2019/2022)
  * [ ] Eclipse
  * [ ] NetBeans
  * [ ] JetBrains
  * [x] [neovim](#neovim)
  * [x] [helix](#helix)
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

## Contributing

See [CONTRIBUTING](./CONTRIBUTING.md) for details.

## Logo

The Contextive logo is based (with extreme gratitude!) on the `Bibliophile` image from https://undraw.co - an amazing resource of free images and illustrations.

## License

This project is licensed under the [MIT](https://choosealicense.com/licenses/mit/) license.  See [LICENSE](LICENSE).
