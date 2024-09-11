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