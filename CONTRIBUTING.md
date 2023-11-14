# CONTRIBUTING

If you have ideas for features or implementation, please open an issue to record your thoughts.

## Commits

Commits follow the [Conventional Commits](https://www.conventionalcommits.org/) specification.  These are used to automatically generate release notes.

## Pull Requests

PRs welcome, but as things are evolving rapidly, a conversation before any major changes is probably worthwhile.

If your PR involves multiple commits, they will most likely be squashed into a single commit so that there is a single line in the release notes, so please title the PR using the conventional commit message that should be used for that single commit.

### Testing

For the most part, Contextive has been developed using Test-Driven Development.  This maintains a high quality test suite with minimal manual regression testing when releasing new versions. 

Although contributors are not required to use Test-Driven Development, we do request appropriate tests be included in any PRs.

### Formatting

All F# code should be formatted with [Fantomas](https://github.com/fsprojects/fantomas).  If you use Visual Studio Code with the DevContainer (see below) this should happen automatically on save.

## Documentation

### IDE Plugins

Each IDE that Contextive supports (either with a plugin, or with a raw language server) has a folder in the (src)[./src] folder.  Please ensure the appropriate `README.md` is updated if your change affects a particular IDE.

### ADRs

Key architectural decisions are tracked using ADRs (Architectural Decision Records) which can be found in the [docs/adr](docs/adr) folder.  The [MADR](https://adr.github.io/madr/) format is the current default.

If your PR or proposal includes major architectural changes, please prepare a draft ADR in that format.

## Development Environment

The default development environment is [Visual Studio Code Dev Containers](https://code.visualstudio.com/docs/remote/containers). After cloning the repository and opening in Visual Studio Code, choose the "Reopen in Container" option to work in the dev environment. See the Visual Studio Code documentation for more details on setting up your docker environment.

If you'd like to setup your own environment, consult the [devcontainer.json](./.devcontainer/devcontainer.json) and [Dockerfile](./.devcontainer/Dockerfile) for details on installed dependencies.