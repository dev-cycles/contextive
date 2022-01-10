# Change Log

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.2-beta]
### Added
- Build for macOS

### Fixed
- Error logged when hovering over a term that isn't in the contextly definitions

## [0.0.1-beta]
### Added
- VsCode command to initialize a contextly definitions file (Default file contains instructions and examples).
- Hover info for terms found in the definitions file
- Auto-complete (with case-matching) for terms found in the definitions file
- Watches for updates to the definitions file
- Watches for updates to the `contextly.path` configuration parameter which defines which file contains the definitions