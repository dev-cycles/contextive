# Contextive README

[![Contextive](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml/badge.svg?branch=release)](https://github.com/dev-cycles/contextive/actions/workflows/contextive.yml?query=branch%3Arelease) [![Twitter](https://img.shields.io/twitter/follow/contextive_tech?label=Follow%20Contextive)](https://twitter.com/intent/follow?screen_name=contextive_tech)

Contextive is a Visual Studio Code extension to assist developers in environments with a complex domain or project specific language, where words have a special meaning in the context of the project.

It should help new team members get up to speed more quickly in understanding domain-specific terms. By storing the term definitions in your repository, and surfacing the definitions as you work on the code, it encourages regularly updating the definitions as the team's understanding evolves.

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
* Hover to show definitions from your Contextive Definitions
* Currently configured to work in files of type: c, cpp, csharp, fsharp, go, groovy, html, java, javascript, javascriptreact, json, jsonc, markdown, perl, php, plaintext, powershell, python, ruby, rust, sql, typescript, typescriptreact, vb, xml, yaml
* Support for word identification in combined usage such as camelCase, PascalCase and snake_case
* Support for documenting combined words (e.g. verbNoun or noun_verbed)

### Combined Words

For the hover display, Contextive is able to identify the use of defined terms in combined words - where terms are combined using `camelCase`, `PascalCase` or `snake_case`, or defined terms that _are_ combined words.

#### Examples

The following examples are drawn from the Cargo domain, as explored by [Eric Evans](https://twitter.com/ericevans0) in his seminal work on DDD - [Domain Driven Design: Tackling Complexity in the Heart of Software](https://www.dddcommunity.org/book/evans_2003/).  The usage examples are drawn from sample conversations in the book - ideally, your usage examples should be exact sentences as said by domain experts.

Consider the following Contextive definitions file:

```
contexts:
  - terms:
    - name: Cargo
      definition: A unit of transportation that needs moving and delivery to its delivery location.
      examples:
        - Multiple Customers are involved with a Cargo, each playing a different role.
        - The Cargo delivery goal is specified.
    - name: Leg
      definition: The movement of a Cargo on a specific vessel from load location to unload location.
      examples:
        - Operations will need to contract handling work based on the expected times for each leg
        - For each leg we'd like to see the vessel voyage, the load and unload location, and time.
    - name: Policy
      definition: A set of rules that the routing service must follow when evaluating legs that confirm to the desired routing specification.
      examples:
        - We need to configure the set of policies that will apply for a specific customer.
    - name: LegMagnitudePolicy
      definition: A policy that helps the routing engine select the legs with the lowest magnitude.
      examples:
        - The leg magnitude policy is selecting the fastest leg, but we need it to select the cheapest leg.
```

##### Suffixes and Prefixes

It's quite common to combine a term from your language, such as `cargo` with a suffix such as `Id` (or `service`, or `factory`, etc.).  If your code includes `cargoId`, `CargoId` or `cargo_id`, Contextive will identify the defined term `cargo` and display the definition and usage examples:

![Example of hovering over a combined word containing a match.](images/cargo_id_example.png)

##### Combining two (or more) terms

It's also common to end up with code elements (classes, variables or methods) that combine two or more terms from your language, such as `Leg` and `Policy`.  Even if you haven't explicitly created a term for `LegPolicy`, Contextive will identify both words and show you both definitions at the same time:

![Example of hovering over a combined word with multiple matches.](images/leg_policy_example.png)

##### Combined words as a single term

Sometimes, the combined term needs it's own unique definition - just add it to your definitions file, and Contextive will work out that the more precise match is the one you want, decluttering your hover panel.  For example, once `LegMagnitudePolicy` is defined, the definitions of `Leg` and `Policy` will no longer be shown:

![Example of hovering over an exactly matching combined word.](images/leg_magnitude_policy_example.png)

#### Limitations

Both of these limitations will be fixed in a future update:

1. `snake_case` doesn't work for terms that are actual combined words, because when looking for an exact match, Contextive just does a case-insensitive string comparison.
   1. E.g. if your code includes `leg_magnitude_policy`, Contextive will not recognise the match, and only show you definitions for `Leg` and `Policy`.
2. Terms that are actual combined words can't yet be found as substrings of a token.
   1. E.g. if code includes `LegMagnitudePolicyFactory`, Contextive will not recognise the match as it splits into `Leg`, `Magnitude` and `Policy` and `Factory`, none of which match the term `LegMagnitudePolicy`.

#### Coming Soon

* UI to edit/manage Contextive Definitions
* Show definitions in auto-complete details
* Internationalization support
* Support for multiple contexts in the same or separate repos
* Configurable list of language identifiers. The list is currently hard coded as above.
* Better support for key word identification in different languages (e.g. different syntax delimiters)
* Support for detecting plural or singular versions of terms
* Enhanced support for combined words, as noted above.
* Support for multi-word defined terms (e.g. with `<space>` between words, rather than a combined word.)

## Extension Settings

This extension contributes the following settings:

* `contextive.path`: The path of the file that stores the Contextive definitions.  Default: `.contextive/definitions.yml`

## Known Issues

* The extension only activates on the presence of the `.contextive` folder in the workspace.  If the `contextive.path` setting has been updated, the `.contextive` folder may not exist.  (The extension will also activate on use of the `Contextive: Initialize Definitions` command.)