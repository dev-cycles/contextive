# Contextive

![Contextive Banner](images/contextive_banner.png)

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive-vscode.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive-vscode.yml) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech) 

Contextive is a suite of tools to immerse developers in the language of their users' domains.

## SURVEY

We're currently conducting a [survey](https://forms.gle/3pJSUYmLHv5RQ1m1A) for Contextive users (or folk interested in Contextive) to help shape the future roadmap and better understand how teams are using Contextive.

It should only take around 10 minutes - we'd really appreciate your thoughts! [Click here](https://forms.gle/3pJSUYmLHv5RQ1m1A) to take the survey.

While the survey is underway, you'll also notice a one-time unobtrusive popup inviting users to help shape the future of Contextive when it is loaded (since v1.10.2).  This popup will be removed when we complete the survey.

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
  * [ ] Define relationships between contexts (e.g. a Context Map definition)
  * [x] Store Ubiquitous Language term definitions in a file in the repository (e.g. yml format)
  * [x] Support a monorepo with multiple Bounded Contexts in one repo - identify which paths relate to each Bounded Context
  * [x] Support a Bounded Context distributed across multiple repos (#36)
  * [x] Support aliases of terms, hovering over the alias shows the term definition
    * [x] Add details of the alias in the hover
    * [ ] Add ability to define an alias as deprecated and warn as such
  * [x] Support multiline domain vision statements, definitions and usage examples
* [ ] IDE Support
  * [x] Visual Studio Code
    * [x] Support single-root workspaces
    * [x] Support multi-root workspaces with a shared definitions file
    * [ ] Support multi-root workspaces with a definitions file per root
  * [ ] Visual Studio (2019/2022)
  * [ ] Eclipse
  * [ ] NetBeans
  * [ ] JetBrains
  * [x] neovim
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

### All in one IDE Plugins

Contextive language server does not need to be installed separately.

#### Visual Studio Code

Open Visual Studio Code, launch the quick open (`Ctrl+P`) and then enter `ext install devcycles.contextive`.  OR, search `contextive` in the extensions side-bar.

Visit the [Contextive Marketplace](https://marketplace.visualstudio.com/items?itemName=devcycles.contextive) page for details.

Check the extension [README](src/vscode/contextive/README.md) for usage instructions.

### Language Server Configurations

#### Installing Contextive Language Server

To install the language server, you need to do it manually from GitHub releases assets. This is necessary for users who are not using the Contextive VSCode extension and Visual Studio Code.

##### 1. Download the appropriate zip file for your operating system and architecture:

```shell
curl -L https://github.com/dev-cycles/contextive/releases/download/<version>/Contextive.LanguageServer-<os>-<arch>-<version>.zip -o Contextive.LanguageServer-<os>-<arch>-<version>.zip
```

##### 2. Unzip the Contextive.LanguageServer and copy the file into a folder that is included in your system's PATH:

The $HOME/bin directory has been created beforehand and is included in the system's PATH.

```shell
unzip Contextive.LanguageServer-<os>-<arch>-<version>.zip -d contextive-language-server
cp contextive-language-server/Contextive.LanguageServer $HOME/bin
```

##### 3. Verify that Contextive.LanguageServer is found in the PATH. A non-zero exit code indicates that the language server was not found in the PATH:

```shell
command -v Contextive.LanguageServer
```

The command should return the absolute path to the binary if it's found in the system PATH.

#### Neovim

How to configure Neovim with lua modules: https://neovim.io/doc/user/lua-guide.html#lua-guide-config

Use lspconfig to create a custom contextive language server configuration and initialize the language server by calling the setup function. The following lua snippet needs to be included in the `init.lua` file either directly or from another lua module like `lspconfigs.lua`.

```lua
local lspconfig = require("lspconfig")

local lspconfig_configs = require("lspconfig.configs")

lspconfig_configs.contextive = {
  default_config = {
    cmd = { "Contextive.LanguageServer" },
    root_dir = lspconfig.util.root_pattern('.contextive', '.git'),
    --settings={contextive={path="./path/to/definitions.yml"} -- uncomment this line to nominate a custom definitions.yml file location
  },
}

lspconfig.contextive.setup {}
```

### Others

Coming soon!

## Contributing

See [CONTRIBUTING](./CONTRIBUTING.md) for details.

## Logo

The Contextive logo is based (with extreme gratitude!) on the `Bibliophile` image from https://undraw.co - an amazing resource of free images and illustrations.

## License

This project is licensed under the [MIT](https://choosealicense.com/licenses/mit/) license.  See [LICENSE](LICENSE).
