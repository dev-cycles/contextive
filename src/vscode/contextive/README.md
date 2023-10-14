# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive-vscode.yml/badge.svg)](https://github.com/dev-cycles/contextive/actions/workflows/contextive-vscode.yml) [![Mastodon](https://img.shields.io/mastodon/follow/111227986489537355?domain=https%3A%2F%2Ftechhub.social%2F
)](https://techhub.social/@contextive) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech)

Contextive is a Visual Studio Code extension to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the term definitions in your repository, and surfacing the definitions as you work on the code, it encourages the use of the domain-specific terms in your code, and regularly updating the definitions as the team's understanding evolves.

![Example of Contextive in action.](images/simple-auto-complete-demo.gif)

## SURVEY

We're currently conducting a [survey](https://forms.gle/3pJSUYmLHv5RQ1m1A) for Contextive users (or folk interested in Contextive) to help shape the future roadmap and better understand how teams are using Contextive.

It should only take around 10 minutes - we'd really appreciate your thoughts! [Click here](https://forms.gle/3pJSUYmLHv5RQ1m1A) to take the survey.

While the survey is underway, you'll also notice a one-time unobtrusive popup when Contextive first starts (since v1.10.2).  This popup will be removed when we complete the survey.

## Getting Started

Use the `Contextive: Initialize Definitions` command from the command palette to create a sample definitions file. A file will be created and opened with a sample set of definitions:

![Example of a Contextive definition hover over the word "context" in a yml file.](images/example_hover.png)

This sample file illustrates the use of Contextive by defining the terms used in the definitions file yml structure itself.  You can hover over the name of the terms in the file to see Contextive in action (see the sample image above).

You should delete the sample definitions and replace them with your own.

## Philosophy

Contextive is inspired by the concept of the [Ubiquitous Language](https://martinfowler.com/bliki/UbiquitousLanguage.html) from the practice of [Domain Driven Design (DDD)](https://martinfowler.com/bliki/DomainDrivenDesign.html) and should support ubiquitous language management practices on DDD projects.

Even if you're not using Domain Driven Design, Contextive should still be very helpful in any software project where it's important that developers are aligned on the meaning of terms.

## Features

* Initialize your Contextive Definitions
* Auto-complete from your Contextive Definitions
  * Shows definitions in auto-complete details
* Hover to show definitions from your Contextive Definitions
  * Hover over usage of multiple terms combined using camelCase, PascalCase and snake_case
  * Hover over multi-word terms
  * Hover over plural of defined terms
  * Hover over aliases of defined terms
* Supported Repository Layouts:
  * A repository per context
  * Multiple contexts in the same repository (monorepo) (identified by path globs)
  * Context distributed over multiple repositories (#36)
* Works in all files (uses the `*` document selector)

### Examples

In the following sections, examples are drawn from the Cargo domain, as explored by [Eric Evans](https://twitter.com/ericevans0) in his seminal work on DDD - [Domain Driven Design: Tackling Complexity in the Heart of Software](https://www.dddcommunity.org/book/evans_2003/).

The usage examples are quoted or inspired by sample conversations in the book - ideally, your usage examples should be exact sentences as said by your domain experts.

The following Contextive definitions file was used to generate all screenshots/scenarios below:

```
contexts:
  - name: Cargo
    domainVisionStatement: To manage the routing of cargo through transportation legs
    paths:
    - CargoDemo
    terms:
    - name: Cargo
      definition: A unit of transportation that needs moving and delivery to its delivery location.
      examples:
        - Multiple Customers are involved with a Cargo, each playing a different role.
        - The Cargo delivery goal is specified.
      aliases:
        - unit
    - name: Leg
      definition: The movement of a Cargo on a specific vessel from load location to unload location.
      examples:
        - Operations will need to contract handling work based on the expected times for each leg
        - For each leg we'd like to see the vessel voyage, the load and unload location, and time.
    - name: Policy
      definition: |
        A set of rules that the routing service must follow
        when evaluating legs that confirm to the desired routing specification.
      examples:
        - We need to configure the set of policies that will apply for a specific customer.
    - name: Leg Magnitude Policy
      definition: A policy that helps the routing engine select the legs with the lowest magnitude.
      examples:
        - The leg magnitude policy is selecting the fastest leg, but we need it to select the cheapest leg.
    - name: Vessel
  - name: Billing
    domainVisionStatement: Compute and levy charges for shipping
    paths:
    - BillingDemo
    terms:
    - name: Policy
      definition: A set of payment rules that defines when invoices are due, and actions to take when unpaid.
      examples:
        - The billing policy is to send to collections after 90 days in arrears.
```

### Combined Words

For the hover display, Contextive is able to identify the use of defined terms in combined words - where terms are combined using `camelCase`, `PascalCase` or `snake_case`, or defined terms that _are_ combined words.

##### Suffixes and Prefixes

It's quite common to combine a term from your language, such as `cargo` with a suffix such as `Id` (or `service`, or `factory`, etc.).  If your code includes `cargoId`, `CargoId` or `cargo_id`, Contextive will identify the defined term `cargo` and display the definition and usage examples:

![Example of hovering over a combined word containing a match.](images/simple-auto-complete-demo.gif)

##### Combining two (or more) terms

It's also common to end up with code elements (classes, variables or methods) that combine two or more terms from your language, such as `Leg` and `Policy`.  Even if you haven't explicitly created a term for `LegPolicy`, Contextive will identify both words and show you both definitions at the same time:

![Example of hovering over a combined word with multiple matches.](images/multi-match-auto-complete-demo.gif)

##### Multi-word terms

Sometimes, the combined term needs its own unique definition - just add it to your definitions file, and Contextive will work out that the more precise match is the one you want, decluttering your hover panel.  

It can be added to your definitions file as either separate words (e.g. `Leg Magnitude Policy`) or as `PascalCase` or `camelCase` (e.g. `LegMagnitudePolicy`).  Either way, once it's defined, the definitions of `Leg` and `Policy` will no longer be shown:

![Example of hovering over an exactly matching combined word.](images/leg_magnitude_policy_example.png)

This also now works for `snake_case` code:

![Example of snake_case working in auto-complete and hover.](images/snake-case-auto-complete-demo.gif)

### Plural Words

Contextive can detect a defined term even when it is defined in the singular and used in the plural.

Coming Soon: Ability to detect a defined term when it is defined in the plural and used in the singular, and when individual words in a multi-word term are pluralised or singularised.

### Term Aliases

Contextive supports defining a list of aliases for a given term.  These can be acronyms or just alternative words that are sometimes used interchangeably.  For example, in the cargo domain above, `unit` is an alias of `cargo`.

When hovering over the word `unit`, the definition of `cargo` will be displayed:

![Example of hovering over the alias 'unit' for the term 'cargo'.](images/alias.png)

### Multiline YAML

As some of the fields in the definitions file could have long text, it may be helpful to use multi-line yaml. Because the fields are interpreted as markdown, there are some special considerations to be aware of.  The site https://yaml-multiline.info/ is a great resource for multi-line yaml fields.

The following common scenarios are more fully explored in the [multiline sample file](test/single-root/fixtures/simple_workspace/.contextive/multiline.yml) and apply to all fields, but are most likely useful for the `domainVisionStatement`, `terms.definition` and `terms.examples` fields.

#### Multiline with Line Break

If a definition (or domain vision statement) is quite long, and you would like to include linebreaks (but not new paragraphs), you need to use one of the yaml multi-line options that ensures at least one newline is parsed, AND to render a markdown linebreak you need to add two `<SPACE>` characters at the end of the line:

```yaml
contexts:
  - name: Demo Multiline Options
    terms:
      - name: LiteralLineBreak
        definition: |
          This definition has multiple lines with a linebreak  
          achieved using the literal block indicator, a single newline, and two spaces at the end of the first line
```

Renders to:

![Example of literal multi-line with linebreak.](images/multiline_literal_linebreak.png)

#### Multiline with New Paragraph

To achieve a new paragraph, use a yaml option that parses multiple newlines, e.g.:

```yaml
contexts:
  - name: Demo Multiline Options
    terms:
      - name: LiteralNewParagraph
        definition: |
          This definition has multiple paragraphs

          achieved using the literal block indicator and two newlines
```

Renders to:

![Example of literal multi-line with new paragraph.](images/multiline_literal_newpara.png)

#### Multiline in No Newline in Hover

Sometimes you may want to break over multiple lines to make it easier to read the raw yaml, but don't want the newlines to appear in the hover.  In this case, just don't use any Markdown linebreak or new para features.  e.g.:

```yaml
contexts:
  - name: Demo Multiline Options
      - name: LiteralMultilineNoBreak
        definition: |
          This definition has no break in the hover panel
          Even though it's over multiple lines, because there are neither 2 spaces (linebreak) nor two newlines (new paragraph)
```

Renders to:

![Example of literal multi-line with no break.](images/multiline_literal_nobreak.png)

The line will still wrap according to the width of the hover panel.

### Smart Auto-Complete

As the terms added to the auto-complete are from a definitions file, not from your code symbols, the auto-complete will work in any file of any language - including documentation, such as markdown.

To ensure it's useful in a variety of scenarios, it includes a number of options to fit your required format:

* camelCase
* PascalCase
* snake_case
* UPPER_CASE

The auto-complete options will adjust as you type - e.g. after typing a single lower-case letter, only `camelCase` and `snake_case` will be included.  After typing a single upper case letter, `PascalCase` and `UPPER_CASE` will be included.  After typing two upper case letters, single word, snake_case and combined words will all be in `UPPERCASE`.

![Example of smart auto-complete.](images/markdown-demo.gif)

### Repository Organisation

Contextive is designed to work with a variety of repository organisation schemes:

#### Single Bounded Context, Multiple Repositories

When a single context is comprised of services, each with their own repository, it's not ideal to have to maintain a copy of the context's language definitions in each repository.

To facilitate this pattern, Contextive recommends storing the terms only once, in a central/common repository, and using native package management facilities to distribute the definitions file as part of any common code packages.

For package managers that store the packages within the workspace, simply set `contextive.path` to the relative path of the downloaded package.  For package managers that store the packages in a global cache, Contextive allows the `contextive.path` setting to contain a shell escape code - `$(cmd)`. When `$(` is detected in the path setting, Contextive will execute `echo "%contextive.path%"` and use the result to `stdout` as the location of the definitions file.

See https://github.com/dev-cycles/contextive-demo-go-service for an example of this in action using `golang` packages.


#### Multiple Bounded Contexts, Repository per Context

This is the simplest option - all you need to do is define the terms for that context in the relevant repository.

#### Multiple Bounded Contexts, Single Repository, Single-Root (Monorepo)

For projects utilising a [monorepo](https://en.wikipedia.org/wiki/Monorepo) it's not uncommon to have code relating to multiple [bounded contexts](https://martinfowler.com/bliki/BoundedContext.html) in the same repository.  At this time, Contextive tracks all definitions in the same file.  Each context has a `paths` property that defines a list of path [globs](https://en.wikipedia.org/wiki/Glob_(programming)).  When working on a file, any context with a matching path glob will be evaluated when finding matches for hover and auto-complete.

This is particularly helpful if the same term (e.g. a common term, like `client`, or `invoice`) is used in multiple contexts.  The definition in each context can relate specifically to its usage in that context.

Each context has optional properties `name` and `domainVisionStatement`.  When set, these names and vision statements will be included in the hover panel.

Given the definitions file above, and a folder structure like so:

```
/CargoDemo
  /LegPolicy.cs
/BillingDemo
  /Policy.cs
```

Contextive will match the files to the path glob configurations.

When hovering over `CargoDemo/LegPolicy.cs` we get:

![Example of hovering over policy in the Cargo context.](images/leg_policy_example.png)

When hovering over `BillingDemo/Policy.cs` we get:

![Example of hovering over policy in the Billing context.](images/billing_policy_example.png)

#### Multiple Bounded Contexts, Multi-Root, Shared Definitions File

Visual Studio Code supports a [multi-root workspace](https://code.visualstudio.com/docs/editor/multi-root-workspaces) configuration which allows separate folders to be selected as 'roots' of a workspace.

Contextive supports this configuration as long as the definitions for all roots are in a shared file.  (Support for separate files per root is a future enhancement.)

The Contextive definitions file is located at `.contextive/definitions.yml` by default - as this is a relative path, in a single-root configuration it is relative to the workspace root. In the multi-root configuration, the definitions file must be located relative to the folder of the `.code-workspaces` file to avoid ambiguity about which root it is relative to.

The `paths` in the definitions file relate to the paths of the roots on disk, not to their names in the vscode multi-root configuration.

If you'd like to store the definitions file in a different location, the appropriate settings location to use is the `settings` key in the `.code-workspaces` file itself, as this will apply in all roots.

### Coming Soon

* UI to edit/manage Contextive Definitions
* Internationalization support
* Support for multiple contexts in separate repositories
* Better support for key word identification in different languages (e.g. different syntax delimiters)

## Extension Settings

This extension contributes the following settings:

* `contextive.path`: The path of the file that stores the Contextive definitions.  Default: `.contextive/definitions.yml`

## Known Issues

* The extension only activates on the presence of the `.contextive` folder in the workspace.  If the `contextive.path` setting has been updated, the `.contextive` folder may not exist.  (The extension will also activate on use of the `Contextive: Initialize Definitions` command.)