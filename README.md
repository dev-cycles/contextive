# Contextly

Contextly is a suite of tools to support the project-wide use of a Ubiquitous Language for software projects following Domain Driven Design.

## Proposed Features

The following list of features is a draft proposal of the vision at the start of the project.  It's expected that the list will evolve as a prototype is developed and experimented with and lessons are learnt.

* [ ] Ubiquitous Language Definition & Storage:
  * [ ] Classify terms as aggregates/entities, attributes, commands and events
  * [x] For each term, include a definition, and examples of usage in normal sentences
  * [ ] Link terms to each other (e.g. commands to the aggregates/entities they apply to; events to the aggregates/entities that publish them)
  * [x] Store Ubiquitous Language term definitions in a file in the repository (e.g. yml format)
  * [ ] Store terms by Bounded Context, and identify which repos/paths relate to each Bounded Context
* [ ] Code-editing Features
  * [x] Show the term definition when hovering over the word in the editor 
  * [x] Add Ubiquitous Language terms to the auto-complete list
  * [ ] Codelens to identify the number of Ubiquitous Language terms in a method or class
  * [ ] Problems/warnings if misuse of the Ubiquitous Language - e.g. use of Command/Event and aggregate in the same class or method name when they are not linked, or use of extra words next to an Aggregate that _isn't_ defined
  * [ ] In relevant places (e.g. hover), note the use of the term in other contexts to highlight the contrast and ensure clarity of which definition applies in the current context (e.g. '_THIS_ definition, _NOT_ that other definition that you might have been thinking of')
* [ ] Ubiquitous Language Management Features
  * [ ] A UI widget to view the language terms in a TreeView and offer facilities for adding, updating & removing terms
  * [ ] Make it easy to add terms to the Ubiquitous Language from existing code e.g. by highlighting and using the right-click menu

## Status

The project is just getting started, so there aren't a lot of details yet - if this project is of interest, get in touch or check back soon!

[![Contextly](https://github.com/dev-cycles/contextly/actions/workflows/contextly.yml/badge.svg)](https://github.com/dev-cycles/contextly/actions/workflows/contextly.yml)

## Installation

It's expected that Contextly will be available via the Extension/Plugin Marketplaces of the various IDEs that will be supported.

## Contributing

As there isn't much here yet, pull requests won't have much to contribute against. If you have ideas for features or implementation, please open an issue to record your thoughts.

Key architectural decisions are tracked using ADRs (Architectural Decision Records) which can be found in the [docs/adr](docs/adr) folder.  The [MADR](https://adr.github.io/madr/) format is the current default.

Development is being done using [Visual Studio Code Dev Containers](https://code.visualstudio.com/docs/remote/containers). After cloning the repository and opening in Visual Studio Code, choose the "Reopen in Container" option to work in the dev environment. See the Visual Studio Code documentation for more details on setting up your docker environment.

## License
[MIT](https://choosealicense.com/licenses/mit/)