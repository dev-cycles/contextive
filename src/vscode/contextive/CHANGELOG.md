# Change Log

## [1.2.1](https://github.com/dev-cycles/contextive/compare/v1.2.0...v1.2.1) (2022-01-28)


### Bug Fixes

* **language-server:** add emojis to the hover panel to illustrate definitions and usage examples ([634b1df](https://github.com/dev-cycles/contextive/commit/634b1df2f994d96efb2201259c3c3296860fe757))

# [1.2.0](https://github.com/dev-cycles/contextive/compare/v1.1.3...v1.2.0) (2022-01-26)


### Bug Fixes

* **language-server:** language server version updated on release ([#26](https://github.com/dev-cycles/contextive/issues/26)) ([2a626a7](https://github.com/dev-cycles/contextive/commit/2a626a7f2285cb6eb73878aa90ff8149b49d1e38))


### Features

* **language-server:** hover now supports camelCase, PascalCase or snake_case words where defined terms are combined with other defined terms, or undefined terms ([#27](https://github.com/dev-cycles/contextive/issues/27)) ([19d9c6c](https://github.com/dev-cycles/contextive/commit/19d9c6c69c9484140b639bf4d0a430a20cd788fc))

## [1.1.3](https://github.com/dev-cycles/contextive/compare/v1.1.2...v1.1.3) (2022-01-24)


### Bug Fixes

* **vscode:** ensure Language Server version is updated on release, so it is reported in vscode extension logs correctly. ([4e377f5](https://github.com/dev-cycles/contextive/commit/4e377f52e24e159b78a686f70dfd62809fb18674))
* **vscode:** resilient to more types of parsing errors ([4f8dde6](https://github.com/dev-cycles/contextive/commit/4f8dde686abac1de20949c1e4e3c03a4fd848e1f))

## [1.1.2](https://github.com/dev-cycles/contextive/compare/v1.1.1...v1.1.2) (2022-01-23)


### Bug Fixes

* **vscode:** add support for Apple Silicon ([1e65def](https://github.com/dev-cycles/contextive/commit/1e65def2c8afa94d140f62da882e52c7e74ade01))

## [1.1.1](https://github.com/dev-cycles/contextive/compare/v1.1.0...v1.1.1) (2022-01-23)


### Bug Fixes

* **vscode:** ensure Language Server version is updated on release ([44e8cb3](https://github.com/dev-cycles/contextive/commit/44e8cb3293c3c0e5666f40dcb2556fe9389f6ffe))
* **vscode:** Readme image relative link corrected to ensure readme images are displayed ([b453ff6](https://github.com/dev-cycles/contextive/commit/b453ff6ed71bea2e87b0015432944ed0393c5242))

# [1.1.0](https://github.com/dev-cycles/contextive/compare/v1.0.0...v1.1.0) (2022-01-23)


### Bug Fixes

* **devcontainer:** Resolve adr template default setting by ensuring /home/vscode/.config exists in advance, and running scripts with  to ensure profile env variables are loaded. ([#20](https://github.com/dev-cycles/contextive/issues/20)) ([9c4639e](https://github.com/dev-cycles/contextive/commit/9c4639e6c4ab7f3845e0c403a9c50d76ec4df9a4))
* handle invalid definitions file ([#21](https://github.com/dev-cycles/contextive/issues/21)) ([caba982](https://github.com/dev-cycles/contextive/commit/caba98230ee995177bfa098b4f8604f09c640da5))


### Features

* Add more word boundary delimiters (arrays, parentheses, parens) ([#18](https://github.com/dev-cycles/contextive/issues/18)) ([5c6d6be](https://github.com/dev-cycles/contextive/commit/5c6d6be5b854833ac278b17804838bfd27d0cd06))

# [v1.0.0](https://github.com/dev-cycles/contextive/compare/v0.0.2-beta...v1.0.0) (2022-01-22)

This is a release label attached to the existing v0.0.1-beta release, in order to serve as a baseline for starting to use [semantic-release](https://semantic-release.gitbook.io/semantic-release/) to manage releases.

## [v0.0.2-beta](https://github.com/dev-cycles/contextive/compare/v0.0.1-beta...v0.0.2-beta) (2022-01-10)


### Features

- Build for macOS

### Fixed

- Error logged when hovering over a term that isn't in the contextive definitions

## [v0.0.1-beta](https://github.com/dev-cycles/contextive/compare/v0.0.1-beta...v0.0.2-beta) (2022-01-09)


### Features

- VsCode command to initialize a contextive definitions file (Default file contains instructions and examples).
- Hover info for terms found in the definitions file
- Auto-complete (with case-matching) for terms found in the definitions file
- Watches for updates to the definitions file
- Watches for updates to the `contextive.path` configuration parameter which defines which file contains the definitions
