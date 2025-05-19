# 🚀 [1.17.0](https://github.com/dev-cycles/contextive/releases/tag/v1.17.0) (2025-05-19)

[📘 Docs](https://docs.contextive.tech/community/v/1.17.0) | [📋 Compare with v1.16.2](https://github.com/dev-cycles/contextive/compare/v1.16.2...v1.17.0)

### 🌟 Features

* **language-server:** support .yaml extension as well as .yml in glossary filenames (fixes [#106](https://github.com/dev-cycles/contextive/issues/106)) ([7128344](https://github.com/dev-cycles/contextive/commit/7128344e4c0289fdb6c39d66601064b736eb223c))
* **language-server:** support diacritic markers in defined terms that are not present in code ([#95](https://github.com/dev-cycles/contextive/issues/95)) ([c961ccc](https://github.com/dev-cycles/contextive/commit/c961ccc9bffe95954a37cbfae4a85f5ee7a3a18e))
* **language-server:** support ẞ and ß in terminology definitions when represented as 'ss' in code ([#95](https://github.com/dev-cycles/contextive/issues/95)) ([36673f4](https://github.com/dev-cycles/contextive/commit/36673f46e0284fd3b62257ead6ab3237aad75f3d))
* **vscode:** activate vscode extension on detecting .glossary.yaml extension ([#106](https://github.com/dev-cycles/contextive/issues/106)) ([51f02b5](https://github.com/dev-cycles/contextive/commit/51f02b5d93bdfdf5d7460931e59398bf8f53b08d))

### 📈 Performance Enhancement

* **language-server:** Improve performance of auto-completion with many very large glossaries (tested with 15 glossaries of 10,000 terms each) ([14dff7c](https://github.com/dev-cycles/contextive/commit/14dff7c9cd44c3111f8fbc01fad3c461b36e047c))
* **language-server:** Pre-index glossaries to improve performance with many very large glossaries (tested with 15 glossaries of 10,000 terms each) ([f230f7f](https://github.com/dev-cycles/contextive/commit/f230f7f1a31e40cbbf6d2b54318fd5343852e47c))
