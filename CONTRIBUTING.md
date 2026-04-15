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

The default development environment uses [devenv](https://devenv.sh), an environment management tool based on nix.

The simplest way to get started is to ensure `devenv` is [installed](https://devenv.sh/getting-started/) (Note: the instructions say to install Nix first - [Determinate Nix](https://docs.determinate.systems/determinate-nix/) is also a valid option that works well for the Contextive maintainers).

Then, run:

`./start.sh` in the root of the repo.

Once the environment is available you'll be in a shell with all necessary dependencies.  Launch your favourite IDE within that shell and get started.

README.md within each component provides guidance on building and testing each component.