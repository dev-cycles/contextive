# General Architecture 

* Status: accepted
* Deciders: Chris Simon
* Date: 2021-10-23

## Decision Summary

Use a common Language Server following the [Language Server Protocol](https://microsoft.github.io/language-server-protocol/) to ensure effective re-use across multiple IDEs.

## Context and Problem Statement

The vision of Ubictionary is to provide a suite of tools to assist with the consistent usage of a Ubiquitous Language in code and documentation. The concept of a [Ubiquitous Language](https://martinfowler.com/bliki/UbiquitousLanguage.html) is drawn directly from the discipline of [Domain Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html).

To ensure the ubiquity of the language it is important that the Ubictionary tools are useful and usable in a variety of contexts.

This decision record captures the thinking around how best to ensure this as the project gets started.

## Decision Drivers

* Developers work across a number of IDEs (e.g. Visual Studio 2019/2022, Visual Studio Code, IntelliJ, Eclipse, NetBeans, vim, emacs, etc.)
* The various IDEs have a range of extension development languages/frameworks
* Ubictionary is going to have a small development team, so we will prioritize reuse of components over independent ownership to start with.

## Considered Options

1. Completely separate implementations per IDE
1. Common Language Server
   1. Use the [Language Server Protocol](https://microsoft.github.io/language-server-protocol/) to allow minimal implementations per IDE which communicate with the Language Server

## Decision Outcome

Chosen option: "Common Language Server" using the "Language Server Protocol", because it is *probably* the option that will allow Ubictionary to be useful in more IDEs with less effort.

### Positive Consequences

* Generally the same positive consequences as described in the Language Server Protocol documentation as the reason for its existence

### Negative Consequences

* Constrained by the capabilities of the Language Server Protocol
* More complexity when deploying extensions, e.g.:
   * still need to determine if the Ubictionary Language Server will be bundled with extensions or need to be downloaded separately?
   * what if a user is using multiple IDEs, will they share a Language Server Process, or have separate processes?
* Use of Language Server Protocol Custom Messages may have varying levels of support in different IDE Frameworks

## Pros and Cons of the Options

### Separate Implementations per IDE

* Good, because it would allow a fully customizable experience in each IDE, allowing to make the most of each IDEs capabilities
* Good, because issues in one extension would not affect other extensions
* Bad, there would potentially be a lot of duplicate logic across multiple languages, e.g. Typescript, .NET, Java
* Bad, because inevitably the supported feature set would drive across IDE implementations

### Language Server Protocol

* Good, because it should be easier to add Ubictionary to more IDEs
* Good, because it follows a standard and familiar protocol structure for IDE extension developers
* Good, because new features added to the Language Server will immediately be supported in all IDEs
* Bad, because deploying the Language Server with the IDE extension (and managing updates) adds an extra level of complexity