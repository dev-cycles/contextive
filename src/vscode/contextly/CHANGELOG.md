# Change Log

# [1.1.0](https://github.com/dev-cycles/contextly/compare/v1.0.0...v1.1.0) (2022-01-22)


### Bug Fixes

* **devcontainer:** Resolve adr template default setting by ensuring /home/vscode/.config exists in advance, and running scripts with  to ensure profile env variables are loaded. ([#20](https://github.com/dev-cycles/contextly/issues/20)) ([9c4639e](https://github.com/dev-cycles/contextly/commit/9c4639e6c4ab7f3845e0c403a9c50d76ec4df9a4))
* extract version using jq and github action exec with output ([03068a8](https://github.com/dev-cycles/contextly/commit/03068a83c96f4b41bc4770231b6410783c276def))
* get correct assembly name for reporting language server name ([018d089](https://github.com/dev-cycles/contextly/commit/018d089e7f11c635c25ff2780652b6b68cf88d24))
* handle invalid definitions file ([#21](https://github.com/dev-cycles/contextly/issues/21)) ([caba982](https://github.com/dev-cycles/contextly/commit/caba98230ee995177bfa098b4f8604f09c640da5))
* update language server version, and emit log ([3ec115f](https://github.com/dev-cycles/contextly/commit/3ec115f408b02e87997b6701f3f5e0239fa4aa8f))
* upload artifact to release, trying a different action ([f4a5b0a](https://github.com/dev-cycles/contextly/commit/f4a5b0a5cd0f273328f268cf8d30856f6381c62b))


### Features

* Add more word boundary delimiters (arrays, parentheses, parens) ([#18](https://github.com/dev-cycles/contextly/issues/18)) ([5c6d6be](https://github.com/dev-cycles/contextly/commit/5c6d6be5b854833ac278b17804838bfd27d0cd06))

# [1.1.0-test.3](https://github.com/dev-cycles/contextly/compare/v1.1.0-test.2...v1.1.0-test.3) (2022-01-22)


### Bug Fixes

* extract version using jq and github action exec with output ([03068a8](https://github.com/dev-cycles/contextly/commit/03068a83c96f4b41bc4770231b6410783c276def))

# [1.1.0-test.2](https://github.com/dev-cycles/contextly/compare/v1.1.0-test.1...v1.1.0-test.2) (2022-01-22)


### Bug Fixes

* upload artifact to release, trying a different action ([f4a5b0a](https://github.com/dev-cycles/contextly/commit/f4a5b0a5cd0f273328f268cf8d30856f6381c62b))

# [1.1.0-test.1](https://github.com/dev-cycles/contextly/compare/v1.0.0...v1.1.0-test.1) (2022-01-22)


### Bug Fixes

* **devcontainer:** Resolve adr template default setting by ensuring /home/vscode/.config exists in advance, and running scripts with  to ensure profile env variables are loaded. ([#20](https://github.com/dev-cycles/contextly/issues/20)) ([9c4639e](https://github.com/dev-cycles/contextly/commit/9c4639e6c4ab7f3845e0c403a9c50d76ec4df9a4))
* handle invalid definitions file ([#21](https://github.com/dev-cycles/contextly/issues/21)) ([caba982](https://github.com/dev-cycles/contextly/commit/caba98230ee995177bfa098b4f8604f09c640da5))


### Features

* Add more word boundary delimiters (arrays, parentheses, parens) ([#18](https://github.com/dev-cycles/contextly/issues/18)) ([5c6d6be](https://github.com/dev-cycles/contextly/commit/5c6d6be5b854833ac278b17804838bfd27d0cd06))

# [v0.0.1](https://github.com/dev-cycles/contextly/compare/v0.0.2-beta...v1.0.0) (2022-01-22)

This is a release label attached to the existing v0.0.1-beta release, in order to serve as a baseline for [semantic-release](https://semantic-release.gitbook.io/semantic-release/).

# [v0.0.2-beta](https://github.com/dev-cycles/contextly/compare/v0.0.1-beta...v0.0.2-beta) (2022-01-10)


### Features

- Build for macOS

### Fixed

- Error logged when hovering over a term that isn't in the contextly definitions

# [v0.0.1-beta](https://github.com/dev-cycles/contextly/compare/v0.0.1-beta...v0.0.2-beta) (2022-01-09)


### Features

- VsCode command to initialize a contextly definitions file (Default file contains instructions and examples).
- Hover info for terms found in the definitions file
- Auto-complete (with case-matching) for terms found in the definitions file
- Watches for updates to the definitions file
- Watches for updates to the `contextly.path` configuration parameter which defines which file contains the definitions
