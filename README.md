# Ubictionary

Ubictionary is a project to support the ubiquitous use of a Ubiquitous Language on software projects following Domain Driven Design.

## Proposed Features

The following list of features is a draft proposed list at the onset of the project.  It's expected that the list will evolve as a prototype is developed and experimented with.

* [ ] Manage Ubiquitous Language term definitions in a file in the repository (e.g. yml format)
* [ ] Classify terms as aggregates/entities, attributes, commands and events
* [ ] For each term, include a definition, and examples of usage in normal sentences
* [ ] Link terms to each other (e.g. commands to the aggregates/entities they apply to; events to the aggregates/entities that publish them)
* [ ] Show the term definition when hovering over the word in the editor 
* [ ] Add Ubiquitous Language terms to the auto-complete list
* [ ] A UI widget to view the language terms in a TreeView and offer facilities for adding, updating & removing terms
* [ ] Codelens to identify the number of Ubiquitous Language terms in a method or class
* [ ] Problems/warnings if mismatched terms are used in the same class/file, or terms are misspelled (some overlap with, e.g. cspell) (what level of programming language awareness is required?)
* [ ] Context-aware definitions
  * [ ] Contexts identified by path globs
  * [ ] Note when a term has an alternative definition in another context and highlight the contrast to ensure clarity of which definition applies in the current context

## Status

The project is just getting started, so there aren't a lot of details yet - if this project is of interest, get in touch or check back soon!

## Installation

It's expected that Ubictionary will be available via the Extension/Plugin Marketplaces of the various IDEs that will be supported.

## Contributing

As there isn't much here yet, pull requests won't have much to contribute against. If you have ideas for features or implementation, please open an issue to record your thoughts.

Key architectural decisions are tracked using ADRs (Architectural Decision Records) which can be found in the [docs/adr](docs/adr) folder.  The [MADR](https://adr.github.io/madr/) format is the current default.

Development is being done using [Visual Studio Code Dev Containers](https://code.visualstudio.com/docs/remote/containers). After cloning the repository and opening in Visual Studio Code, choose the "Reopen in Container" option to work in the dev environment. See the Visual Studio Code documentation for more details on setting up your docker environment.

## License
[MIT](https://choosealicense.com/licenses/mit/)