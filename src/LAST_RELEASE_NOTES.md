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
