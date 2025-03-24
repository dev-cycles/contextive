---
title: Change Log
tableOfContents:
  minHeadingLevel: 1
  maxHeadingLevel: 2
---

## ⚡ [1.16.2](https://github.com/dev-cycles/contextive/releases/tag/v1.16.2) (2025-03-24)

[📘 Docs](https://docs.contextive.tech/community/v/1.16.2) | [📋 Compare with v1.16.1](https://github.com/dev-cycles/contextive/compare/v1.16.1...v1.16.2)

### 🐛 Bug Fixes

* **intellij:** fix issue if system property jna.noclasspath is null ([8cfacd6](https://github.com/dev-cycles/contextive/commit/8cfacd6d23d5d591b8c38aa96f4e0cf59c6e590c))

## ⚡ [1.16.1](https://github.com/dev-cycles/contextive/releases/tag/v1.16.1) (2025-03-23)

[📘 Docs](https://docs.contextive.tech/community/v/1.16.1) | [📋 Compare with v1.16.0](https://github.com/dev-cycles/contextive/compare/v1.16.0...v1.16.1)

### 🐛 Bug Fixes

* **language-server:** correct handling of multi-word terms with unicode characters (fixes [#90](https://github.com/dev-cycles/contextive/issues/90)) ([6ed7bc1](https://github.com/dev-cycles/contextive/commit/6ed7bc1d7d957a9f27003fd363a53ee3f282cf10))

# 🚀 [1.16.0](https://github.com/dev-cycles/contextive/releases/tag/v1.16.0) (2025-03-22)

[📘 Docs](https://docs.contextive.tech/community/v/1.16.0) | [📋 Compare with v1.15.0](https://github.com/dev-cycles/contextive/compare/v1.15.0...v1.16.0)

### 🌟 Features

* **language-server:** can import local glossary files to make terms available in multiple locations ([6915ef2](https://github.com/dev-cycles/contextive/commit/6915ef26c91587d50156c510725cbcfef0617f65))
* **language-server:** can import multiple files, local or remote, into a glossary ([d93f716](https://github.com/dev-cycles/contextive/commit/d93f7169dc6b3c222d744c7dfcb63e37cbbf77d1))
* **language-server:** can import remote glossary files from unauthenticated URLs ([26c73da](https://github.com/dev-cycles/contextive/commit/26c73dac12caa1e0fff0be9feee83811003aac93))
* **language-server:** contexts can have a metadata dictionary that is rendered in the context header ([b63950d](https://github.com/dev-cycles/contextive/commit/b63950dda2aab43a40706bcaf5a1c682fe4a5562))
* **language-server:** terms can have a metadata dictionary which is rendered in the hover panel ([5546749](https://github.com/dev-cycles/contextive/commit/55467490b742b4d48dd81d098775575ad5234cd0))

### 🐛 Bug Fixes

* **language-server:** prevent old default glossaries from still being included after the config value is changed ([4f66fac](https://github.com/dev-cycles/contextive/commit/4f66fac598bcdbf9601f2e756a8fa491a136323d))

# 🚀 [1.15.0](https://github.com/dev-cycles/contextive/releases/tag/v1.15.0) (2025-03-19)

[📘 Docs](https://docs.contextive.tech/ide/v/1.15.0) | [📋 Compare with v1.14.1](https://github.com/dev-cycles/contextive/compare/v1.14.1...v1.15.0)

### 🌟 Features

* **intellij:** activate contextive in all projects to ensure the language server can scan the project for glossary files ([2460ff6](https://github.com/dev-cycles/contextive/commit/2460ff61f0a36e014b859679bf66b04f9974a7c9))
* **language-server:** can now locate glossary files in the folder hierarchy - the definitions in the file will automatically apply to any file in the same folder or any children (closes [#54](https://github.com/dev-cycles/contextive/issues/54)) ([dece2a5](https://github.com/dev-cycles/contextive/commit/dece2a59d44dd09c2e559eaec32df5fcc979ebcc))
* **language-server:** glossary files across the folder hierarchy work with multi-root workspaces [#54](https://github.com/dev-cycles/contextive/issues/54) ([9c05eb7](https://github.com/dev-cycles/contextive/commit/9c05eb7212ea6d2828d26cc61ca3ef9bc83d65a2))
* **language-server:** remove local schema managment as the schema has been submitted to the schema registry ([3bfdf60](https://github.com/dev-cycles/contextive/commit/3bfdf60447a6fea3ff97d67738b0763802c0a910))

### 🐛 Bug Fixes

* correct typo in default glossary file ([24e24e0](https://github.com/dev-cycles/contextive/commit/24e24e0b889b70f671b0d1353e866156ff69bbb9))
* **intellij:** prevent error when starting on windows (fixes [#86](https://github.com/dev-cycles/contextive/issues/86)) ([661351d](https://github.com/dev-cycles/contextive/commit/661351d5e639bce41600fb95ecf7d42316ba3461))
* **language-server:** safely handle glossary files with no contexts defined ([36aedec](https://github.com/dev-cycles/contextive/commit/36aedecbca1da6ba1595afc547cdb4460da4e07e))
* **language-server:** safely handle glossary files with null context ([77fb290](https://github.com/dev-cycles/contextive/commit/77fb29075443655cc88d0b45b28cb4d535b089da))

## [1.14.1](https://github.com/dev-cycles/contextive/compare/v1.14.0...v1.14.1) (2025-03-12)


### Bug Fixes

* **vscode,intellij:** correct links in plugin/extension readme ([2d86d37](https://github.com/dev-cycles/contextive/commit/2d86d37d864b7236bd9774afcc4a99e1b8ecf3a4))

# [1.14.0](https://github.com/dev-cycles/contextive/compare/v1.13.0...v1.14.0) (2025-03-11)


### Bug Fixes

* **intellij:** Support forthcoming IntelliJ Platform 2025 editions ([69e3113](https://github.com/dev-cycles/contextive/commit/69e3113a23eeb9130e116df7a2095b136611121a))


### Features

* **language-server:** add support for hovering over kebab-case terms ([3c02938](https://github.com/dev-cycles/contextive/commit/3c02938b3a0ec4296c0f1afdbc80d36cb1991a06))
* **language-server:** multi-word terms can be defined in the definitions file in snake_case or kebab_case ([4aeffd8](https://github.com/dev-cycles/contextive/commit/4aeffd8c89cebb03a51120ca6c10fa94e1821da1))

## [1.12.1](https://github.com/dev-cycles/contextive/compare/v1.12.0...v1.12.1) (2024-09-20)


### Bug Fixes

* **intellij:** Add support for 2024.3 ([430cfec](https://github.com/dev-cycles/contextive/commit/430cfecce0aa4a85cfeabae018250996985ca48c))

# [1.12.0](https://github.com/dev-cycles/contextive/compare/v1.11.1...v1.12.0) (2024-09-04)


### Bug Fixes

* **intellij:** Add support for 2042.2 series of intellij platform ([08308bf](https://github.com/dev-cycles/contextive/commit/08308bff2056fd1af764d88649de7fe90b8516b1))


### Features

* **language-server:** Add support for LSP Clients that only support rootUri and not workspaces (e.g. Visual Studio) ([7fe11b3](https://github.com/dev-cycles/contextive/commit/7fe11b3831d6f8b8f86d1d10817c7ba50a0163c0))
* **language-server:** Add yaml schema for definitions file ([#74](https://github.com/dev-cycles/contextive/issues/74)) ([65ec44a](https://github.com/dev-cycles/contextive/commit/65ec44a16de20357b69d1662cfc70521298287da))
* **language-server:** Only use 'window/showMessage' if it is supported by the LanguageClient (e.g. Visual Studio does not support it) ([965cb30](https://github.com/dev-cycles/contextive/commit/965cb30539ea05357dfd6cdf4e2bb44406d8a16c))
* **language-server:** validate definitions file for missing term names ([0fb0978](https://github.com/dev-cycles/contextive/commit/0fb0978640f594843d1f8ff25959ea3d0bf729ae))
* **visual-studio:** Add Visual Studio integration ([b052a82](https://github.com/dev-cycles/contextive/commit/b052a82df7df39d518760f8dfcae84771505262f)), closes [#28](https://github.com/dev-cycles/contextive/issues/28)
* **vscode:** Publish to Open-Vsx Marketplace (closes [#80](https://github.com/dev-cycles/contextive/issues/80)) ([2d23775](https://github.com/dev-cycles/contextive/commit/2d23775c6c99468319fdf68d7bad2b1997fad883))

## [1.11.1](https://github.com/dev-cycles/contextive/compare/v1.11.0...v1.11.1) (2024-06-15)


### Bug Fixes

* **intellij:** improve resilience of language-server downloading ([7f9a1ce](https://github.com/dev-cycles/contextive/commit/7f9a1ceb217bad26bbb6aff1c0d9e62e600b1823))
* **intellij:** only attempt to start the Contextive language server if a contextive definitions file is present. (fixes [#64](https://github.com/dev-cycles/contextive/issues/64)) ([0f707eb](https://github.com/dev-cycles/contextive/commit/0f707eb216cbd6b0fcd613a925259c12acec8031))
* **intellij:** show progress indicator when downloading language server ([e2ce467](https://github.com/dev-cycles/contextive/commit/e2ce467cfa303764a44796b05737485d710cd3bb))
* **language-server:** ensure initialization of definitions file works even when located sub-folder doesn't exist. ([e09ccaa](https://github.com/dev-cycles/contextive/commit/e09ccaa6f60f9bc19980ab340b4c095a9ca9565c))
* **language-server:** only show errors when definitions file doesn't exist if configuration is explicitly set ([b730f83](https://github.com/dev-cycles/contextive/commit/b730f838353fc9130b805be2de9ed44c30e46931))
* **vscode:** remove default path config from vscode extension and rely on language server default only ([8fdd26d](https://github.com/dev-cycles/contextive/commit/8fdd26d0c886b0ad9875b0fc619e726137e35a64))
* **vscode:** resolve errors in logs since moving default contextive.path from vscode to the languageServer ([79f4854](https://github.com/dev-cycles/contextive/commit/79f485483a64972014239fffb97003ccf6a252b2))
* **vscode:** resolve race condition when initializing contextive definitions file and activating extension at the same time ('method not found' error) ([dd9f171](https://github.com/dev-cycles/contextive/commit/dd9f171b653bbe31715a09146be037846848de85))

# [1.11.0](https://github.com/dev-cycles/contextive/compare/v1.10.5...v1.11.0) (2024-03-11)


### Features

* **intellij:** Add IntelliJ plugin (closes [#32](https://github.com/dev-cycles/contextive/issues/32)) ([fad50b8](https://github.com/dev-cycles/contextive/commit/fad50b835c2003c02afd023f75ccf510f892c9ff))
* **intellij:** automatically download the language server if it's not found ([38db4b7](https://github.com/dev-cycles/contextive/commit/38db4b759a47b45d2d1b7e5a38b54c4bfdd57370))


### Performance Improvements

* **language-server:** reduce binary size (fixes [#61](https://github.com/dev-cycles/contextive/issues/61)) ([5658484](https://github.com/dev-cycles/contextive/commit/56584848fb9e4281092749212ab143690702f88c))

## [1.10.5](https://github.com/dev-cycles/contextive/compare/v1.10.4...v1.10.5) (2023-11-19)


### Bug Fixes

* exception on startup [#61](https://github.com/dev-cycles/contextive/issues/61) ([2a69c0a](https://github.com/dev-cycles/contextive/commit/2a69c0a764099539e537291a85307fbfd2598dd3))

## [1.10.4](https://github.com/dev-cycles/contextive/compare/v1.10.3...v1.10.4) (2023-11-19)


### Bug Fixes

* **language-server:** improve resilience when client doesn't support showMessageRequest (fixes [#60](https://github.com/dev-cycles/contextive/issues/60)) ([8a1e684](https://github.com/dev-cycles/contextive/commit/8a1e68469173ea737f8ddf91514f92fe83851c25))
* **language-server:** language server now offers a custom lsp command to initialize the default definitions file ([9c7e8a5](https://github.com/dev-cycles/contextive/commit/9c7e8a59dfa179aba806fe183b161ac15df38978))

## [1.10.3](https://github.com/dev-cycles/contextive/compare/v1.10.2...v1.10.3) (2023-11-15)


### Bug Fixes

* **language-server:** support LSP clients that only support `workspace/configuration` ([#58](https://github.com/dev-cycles/contextive/issues/58)) ([7e06396](https://github.com/dev-cycles/contextive/commit/7e0639694982e2db1d51b7965429e4d05c3c69f5))
* **language-server:** survey prompt more resilient to accidentally missing the first prompt ([7acd140](https://github.com/dev-cycles/contextive/commit/7acd1401ee057071f2ca6aaa9d7dba2c73eabd32))

## [1.10.2](https://github.com/dev-cycles/contextive/compare/v1.10.1...v1.10.2) (2023-10-14)


### Bug Fixes

* **language-server:** fix issue with survey prompt ([7196e82](https://github.com/dev-cycles/contextive/commit/7196e822a73d7332024ff6fd94bf540cb503efc3))

## [1.10.1](https://github.com/dev-cycles/contextive/compare/v1.10.0...v1.10.1) (2023-10-14)


### Bug Fixes

* **language-server:** resolve issue with releasing v1.10.0 for macos and windows ([5042b5f](https://github.com/dev-cycles/contextive/commit/5042b5f2598d56c6b041360909da005f8eff2b02))

# [1.10.0](https://github.com/dev-cycles/contextive/compare/v1.9.4...v1.10.0) (2023-10-14)


### Features

* **language-server:** includes a one-time invitation for users to complete a Contextive roadmap survey ([42272d0](https://github.com/dev-cycles/contextive/commit/42272d0221b6a79b0d2e025976aabe4cd02ba874))
* **language-server:** Use the default path defined in contextive if it's not specified in the configuration. ([23f9049](https://github.com/dev-cycles/contextive/commit/23f9049a18c638be69a7a44d0a9cda121673703e))

## [1.9.4](https://github.com/dev-cycles/contextive/compare/v1.9.3...v1.9.4) (2023-08-18)


### Bug Fixes

* **language-server:** improve resilience when terms is an empty list in the definitions file. (fixes: [#48](https://github.com/dev-cycles/contextive/issues/48)) ([65517ef](https://github.com/dev-cycles/contextive/commit/65517eff0e7f95088096d2bd914f814825504a20))

## [1.9.3](https://github.com/dev-cycles/contextive/compare/v1.9.2...v1.9.3) (2023-08-08)


### Bug Fixes

* **language-server:** ensure rendering the domain vision statement is correct even with trailing whitespace/newline. (fixes [#47](https://github.com/dev-cycles/contextive/issues/47)) ([27f86c1](https://github.com/dev-cycles/contextive/commit/27f86c1f7ed909c5609bf0349e641d3a57b302a0))
* **language-server:** term usage examples render within double-quotes correctly even with leading and trailing whitespace/newlines. ([292553f](https://github.com/dev-cycles/contextive/commit/292553fc23b85b2c69af0758f8a8f447dd08c6ad))
* **vscode:** the default definitions file includes a definition of the 'aliases' key ([a983db2](https://github.com/dev-cycles/contextive/commit/a983db2ec66368073c60cce0424650714a632e2d))

## [1.9.2](https://github.com/dev-cycles/contextive/compare/v1.9.1...v1.9.2) (2023-07-04)


### Bug Fixes

* **language-server:** display aliases in the hover panel ([6ba291b](https://github.com/dev-cycles/contextive/commit/6ba291b317fee542f4297e6f096693022ad73420))

## [1.9.1](https://github.com/dev-cycles/contextive/compare/v1.9.0...v1.9.1) (2023-07-03)


### Bug Fixes

* **vscode:** ensure the language server process stops when vscode stops the extension. Bug in dependency `vscode-languageclient` fixed by update. (fixes [#44](https://github.com/dev-cycles/contextive/issues/44)) ([a3f7ed8](https://github.com/dev-cycles/contextive/commit/a3f7ed84f5c22d5ffa899dc033ee6076d8bc54ff))

# [1.9.0](https://github.com/dev-cycles/contextive/compare/v1.8.1...v1.9.0) (2023-06-21)


### Features

* **language-server:** add support for term aliases and show term definition when hovering over an alias ([28103de](https://github.com/dev-cycles/contextive/commit/28103deff1e491455a1660571b999bccc437378f))

## [1.8.1](https://github.com/dev-cycles/contextive/compare/v1.8.0...v1.8.1) (2023-06-13)


### Bug Fixes

* **language-server:** Ensure Contextive works with both CRLF and LF files on Windows ([#40](https://github.com/dev-cycles/contextive/issues/40)) ([9a4d248](https://github.com/dev-cycles/contextive/commit/9a4d24839888dced52a5403123d7bf372fed7622))

# [1.8.0](https://github.com/dev-cycles/contextive/compare/v1.7.0...v1.8.0) (2023-06-11)


### Bug Fixes

* **language-server:** ensure hover works even if the file's path has special characters in it ([076a029](https://github.com/dev-cycles/contextive/commit/076a02918815823302ba464ec565301c237f8088))
* **language-server:** resolve error notifications when hovering over a space in column 0 ([51936b8](https://github.com/dev-cycles/contextive/commit/51936b8d5bc1ae769ac86aa9ab8a29630ff9256d))


### Features

* **vscode:** Add support for linux-arm64 platform ([38d9afb](https://github.com/dev-cycles/contextive/commit/38d9afb54b637203541ae6973f6fe2f6b61e9ee5))

# [1.7.0](https://github.com/dev-cycles/contextive/compare/v1.6.0...v1.7.0) (2023-05-28)


### Features

* **vscode:** add support for multi-root workspaces with a shared definitions file ([#38](https://github.com/dev-cycles/contextive/issues/38)) ([99a4257](https://github.com/dev-cycles/contextive/commit/99a4257120d67fc1fb40f740a6b10310f9d5eada))

# [1.6.0](https://github.com/dev-cycles/contextive/compare/v1.5.1...v1.6.0) (2023-03-28)


### Features

* **language-server:** show hover panel for plural of defined terms ([d45095d](https://github.com/dev-cycles/contextive/commit/d45095d02fb04156eff0dfb081487cae54d2d4be))

## [1.5.1](https://github.com/dev-cycles/contextive/compare/v1.5.0...v1.5.1) (2023-03-24)


### Bug Fixes

* **vscode:** Ensure Contextive hover results appear below more relevant language feature results. ([2e9e40c](https://github.com/dev-cycles/contextive/commit/2e9e40ce77e2e7ce8fc542763712fbb94a3494db))

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
