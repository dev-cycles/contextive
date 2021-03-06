# Change Log

# [1.5.0](https://github.com/dev-cycles/contextive/compare/v1.4.0...v1.5.0) (2022-02-13)


### Features

* **language-server:** allow contextive.path to contain shell commands to enable more complex location discovery scripts [#36](https://github.com/dev-cycles/contextive/issues/36) ([dc17612](https://github.com/dev-cycles/contextive/commit/dc176121a029d21ceb6ad8ee5d3bccb44772f9b2))

# [1.4.0](https://github.com/dev-cycles/contextive/compare/v1.3.1...v1.4.0) (2022-02-05)


### Bug Fixes

* **language-server:** include more useful auto-complete suggestions for multi-word terms ([7a1b4ea](https://github.com/dev-cycles/contextive/commit/7a1b4ea970bdcf3867f69e78990a3807a10a2357))
* **language-server:** update completion item kind to be ([7552c16](https://github.com/dev-cycles/contextive/commit/7552c16de6f151e634e60959a60fd8971f5b47bd))


### Features

* **language-server:** show context name (if any) in auto-complete details ([8c221d1](https://github.com/dev-cycles/contextive/commit/8c221d1a3629b7a6561a54f697c9e416bc304d14))
* **language-server:** show term hover panel content in auto complete documentation window ([4748c86](https://github.com/dev-cycles/contextive/commit/4748c863186b57ce50eb0e5e53d4a7efae8f127e))

## [1.3.1](https://github.com/dev-cycles/contextive/compare/v1.3.0...v1.3.1) (2022-02-01)


### Bug Fixes

* **language-server:** support snake_case combined words ([#35](https://github.com/dev-cycles/contextive/issues/35)) ([abd076a](https://github.com/dev-cycles/contextive/commit/abd076afc7a2d089f00be9d766be3ddf0a02d66c))
* **language-server:** support true multi-word terms, not just camelCase and PascalCase terms ([5fc2f73](https://github.com/dev-cycles/contextive/commit/5fc2f73230dfbe24237eee2767d9d7b001a647d8))

# [1.3.0](https://github.com/dev-cycles/contextive/compare/v1.2.1...v1.3.0) (2022-01-31)


### Bug Fixes

* **language-server:** update emojis for definitions and usage examples ([f08e477](https://github.com/dev-cycles/contextive/commit/f08e477b1c2b7ba48bf811662a8a509a86fe7726))


### Features

* **language-server:** contexts can now be defined with a list of path globs. hover and autocomplete terms will only be shown from contexts where at least one path glob matches the currently open file ([8bf13ee](https://github.com/dev-cycles/contextive/commit/8bf13ee4f4d9d7238c9952de4e136ce185babfea))
* **language-server:** display current context name and domain vision statement in hover panel, if defined. ([9658385](https://github.com/dev-cycles/contextive/commit/9658385809f9f36a21f694ad4654af5e285c5097))

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
