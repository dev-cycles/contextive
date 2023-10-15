# CONTRIBUTING

If you have ideas for features or implementation, please open an issue to record your thoughts.

## Pull Requests

PRs welcome, but as things are evolving rapidly, a conversation before any major changes is probably worthwhile.

For the most part, Contextive has been developed using Test-Driven Development.  This maintains a high quality test suite with minimal manual regression testing when releasing new versions. 

Although contributors are not required to use Test-Driven Development, we do request appropriate tests be included in any PRs.

## Documentation

Key architectural decisions are tracked using ADRs (Architectural Decision Records) which can be found in the [docs/adr](docs/adr) folder.  The [MADR](https://adr.github.io/madr/) format is the current default.

If your PR or proposal includes major architectural changes, please prepare a draft ADR in that format.

## Development Environment

The default development environment is [Visual Studio Code Dev Containers](https://code.visualstudio.com/docs/remote/containers). After cloning the repository and opening in Visual Studio Code, choose the "Reopen in Container" option to work in the dev environment. See the Visual Studio Code documentation for more details on setting up your docker environment.

If you'd like to setup your own environment, consult the [devcontainer.json](./.devcontainer/devcontainer.json) and [Dockerfile](./.devcontainer/Dockerfile) for details on installed dependencies.