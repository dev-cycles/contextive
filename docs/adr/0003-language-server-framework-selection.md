# Language Server Framework Selection 

* Status: accepted
* Deciders: Chris Simon
* Date: 2021-10-29

## Decision Summary

Use [OmniSharp.Extensions.LanguageServer](https://github.com/OmniSharp/csharp-language-server-protocol) to provide a Language Server framework and testing support.

## Context and Problem Statement

The [Language Server Protocol](https://microsoft.github.io/language-server-protocol/) is complex and evolving. There exist multiple libraries intended to support the idiomatic implementation of a language server without having to explicitly implement all aspects of protocol support.

## Decision Drivers

Preferred features of the selected library:
* Minimal implementation overhead
* Language Server Protocol Compliant
* Documentation of example usage
* Support for unit and integration testing

## Considered Options

* [OmniSharp.Extensions.LanguageServer](https://github.com/OmniSharp/csharp-language-server-protocol) 
* [Microsoft.VisualStudio.LanguageServer.Protocol](https://www.nuget.org/packages/Microsoft.VisualStudio.LanguageServer.Protocol/)

## Decision Outcome

Chosen option: `OmniSharp.Extensions.LanguageServer`, because it is the only option with any meaningful documentation or testing support.

### Positive Consequences 

* There is a sample server in the github repo to draw inspiration from
* There is an active community supporting it
* It is open source, so it's inner workings can be inspected, and it may be open to contributions
* It includes components specifically designed to support unit and integration testing

### Negative Consequences

* It is unofficial
* It may not remain compliant as the spec evolves

## Pros and Cons of the Options

### `OmniSharp.Extensions.LanguageServer`

[OmniSharp.Extensions.LanguageServer](https://github.com/OmniSharp/csharp-language-server-protocol) 

See positive & negative consequences above.

### `Microsoft.VisualStudio.LanguageServer.Protocol`

[Microsoft.VisualStudio.LanguageServer.Protocol](https://www.nuget.org/packages/Microsoft.VisualStudio.LanguageServer.Protocol/)

* Good, because it's official from Microsoft
* Bad, because the linked documentation only describes how to use the TypeScript language server libraries
* Bad, because there doesn't appear to be any examples of it in use