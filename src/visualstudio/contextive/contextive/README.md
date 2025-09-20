# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml) [![Github](https://img.shields.io/github/stars/dev-cycles/contextive
)](https://github.com/dev-cycles/contextive) [![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?logo=bluesky&logoColor=fff)](https://bsky.app/profile/contextive.tech) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![LinkedIn](https://custom-icon-badges.demolab.com/badge/LinkedIn-0A66C2?logo=linkedin-white&logoColor=fff)](https://www.linkedin.com/company/contextive-tech)

[📘 Documentation](https://docs.contextive.tech/community/v/1.17.5/) | [🚀 Releases](https://github.com/dev-cycles/contextive/releases) | [✉️ Subscribe for Updates](https://buttondown.com/contextive)

Contextive is a Visual Studio Code extension to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the glossary in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

> [!WARNING]  
> This plugin is considered 'beta' status, as leverages relatively new [Language Server Protocol](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/language-server-provider/language-server-provider?view=vs-2022) support in the [Preview Extensibility Model](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/?view=vs-2022). Some features may not work or may not work as expected. Please [report issues](https://github.com/dev-cycles/contextive/issues/new?assignees=&labels=&projects=&template=bug_report.md&title=) in this project and we will liaise with Microsoft & the Visual Studio team to resolve.
> 
> The main issue for most use cases is that the extension doesn't work when opening a solution file, only when opening a folder.  See [Issue #75](https://github.com/dev-cycles/contextive/issues/75) for details.
>
> See [known issues](https://github.com/dev-cycles/contextive/blob/v1.17.5/src/visualstudio/contextive/contextive/README.md#known-issues) for others.

## Installation

See [Visual Studio Instructions](https://docs.contextive.tech/community/v/1.17.5/guides/installation/#visual-studio-2022).

## Getting Started

See [setting up glossaries](https://docs.contextive.tech/community/v/1.17.5/guides/setting-up-glossaries/) and [defining terminology](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/) usage guides for details on getting started with Contextive.

## Features

* Initialize your Contextive Glossary File
* [Auto-complete](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/#smart-auto-complete) from your Contextive Glossary
  * Shows definitions in auto-complete details
* Hover to show definitions from your Contextive Glossary
  * Hover over elements with [suffixes & prefixes](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/#suffixes-and-prefixes)
  * Hover over usage of [multiple terms](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/#combining-two-or-more-terms) combined using camelCase, PascalCase and snake_case
  * Hover over [multi-word](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/#complex-multi-word-terms) terms
  * Hover over [plural](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/#plurals) of defined terms
  * Hover over [aliases](https://docs.contextive.tech/community/v/1.17.5/guides/defining-terminology/#aliases) of defined terms
* Put your glossaries near the code they support:
  * [Terms relevant for the whole repository](https://docs.contextive.tech/community/v/1.17.5/guides/setting-up-glossaries/#terms-relevant-for-the-whole-repository)
  * [Different terms relevant in different repositories](https://docs.contextive.tech/community/v/1.17.5/guides/setting-up-glossaries/#different-terms-relevant-in-different-repositories)
  * [Terms relevant only in a subfolder of the repository](https://docs.contextive.tech/community/v/1.17.5/guides/setting-up-glossaries/#terms-relevant-only-in-a-subfolder-of-the-repository)

### Coming Soon

* UI to edit/manage Contextive Glossary
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Known Issues

* [Doesn't work when opening a Solution, only when opening a folder](https://github.com/dev-cycles/contextive/issues/75)
* [Definitions don't update in hover panels when the glossary file is updated](https://github.com/dev-cycles/contextive/issues/79)
* [Doesn't work in all file types](https://github.com/dev-cycles/contextive/issues/78)
* [Markdown not supported in hover panel](https://github.com/dev-cycles/contextive/issues/76)

See [All Contextive Visual Studio Issues](https://github.com/dev-cycles/contextive/issues?q=is%3Aissue+is%3Aopen+label%3AVisualStudio).