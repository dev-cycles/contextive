# Documentation Framework

* Status: accepted
* Deciders: Chris Simon
* Date: 2024-11-29

## Decision Summary

To build Contextive documentation using [Starlight](https://starlight.astro.build/).

## Context and Problem Statement

So far, Contextive documentation has been kept in a few places:

1. Extension/plugin README files, visible in their web marketplaces and plugin installation experiences
2. Markdown files in a `wiki` folder in the repository, accessible via the Github UI

As the size of the documentation increases, it would be beneficial to have a more comprehensive documentation system that supports a few goals:

1. Expandable to document new tools in the Contextive ecosystem, such as browser plugins etc.
2. Use the [Diátaxis](https://diataxis.fr/) approach to technical documentation authoring
3. Align with Contextive branding
4. Flexible hosting (not reliant on github)

## Decision Drivers

1. Configuration and content version controlled alongside contextive source code
2. Ease of content creation for small team of devs (i.e. no need to be maintained by folk outside of git)
3. Publishable to free/low cost static site hosting
4. Availability of visually appealing and easy to customise templates/themes
5. Support internationalisation in the future
6. Support ineline code-samples

## Considered Options

Documentation Specific Systems:

* [Starlight](https://starlight.astro.build/)
* [Docusaurus](https://docusaurus.io/)

Other Static Site Generators:

* [Astro](https://astro.build)
* [Fornax](https://github.com/ionide/Fornax)

## Decision Outcome

To build Contextive documentation using [Starlight](https://starlight.astro.build/).

[Docusaurus](https://docusaurus.io/) was the other main contender, and both are very compelling options.  The following analyses and experience reports were considered:

* https://blog.logrocket.com/starlight-vs-docusaurus-building-documentation/
* https://www.tinybird.co/blog-posts/new-docs (although they did not choose Starlight, they included a comprehensive comparison)
* https://barnabas.me/blog/2023/06/starlight/


### Positive Consequences

* Simple & configurable template to get started
* Ease of applying accessible contextive branding using [color theme editor](https://starlight.astro.build/guides/css-and-tailwind/#color-theme-editor)
* Extensible via a range of technologies in the future (React, Vue, Svelte) due to being based on Astro

### Negative Consequences

* Less mature, so less broad ecosystem of extensions and examples, although it is possible to use Astro plugins

## Pros and Cons of the Options

### Starlight

* Good, because easy to get started
* Good, because built on Astro so flexible future pathway
* Good, because great documentation on how to [customise](https://starlight.astro.build/guides/customization/)
* Bad, because less examples and less mature ecosystem

### Docusaurus

* Good, because easy to get started
* Good, because broad range of examples and mature ecosystem
* Good, because good documentation on how to [configure](https://docusaurus.io/docs/configuration)
* Bad, because locked to React

### Astro

* Good, because flexible future pathway
* Bad, because would require more effort to setup documentation specific template

### Fornax

* Good, because F# keeps it in the broad Contextive language ecosystem
* Bad, because immature range of templates and extension examples
