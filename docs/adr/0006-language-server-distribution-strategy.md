# Language Server Distribution Strategy 

* Status: accepted
* Deciders: Chris Simon
* Date: 2022-01-05

## Decision Summary

Build using .NET single-file and self-contained option with separate builds per target platform. Leverage multi-platform VSCode builds to provide an extension per platform.

## Context and Problem Statement

### .NET Publishing Options for the Language Server

As per [ADR-0002](0002-language-&-framework-selection.md), Contextive uses F# and .NET 6.0 for the Language Server.  .NET offers a number of [deployment models](https://docs.microsoft.com/en-us/dotnet/core/deploying/).  The main two modes are:

1. _self-contained_ - an app that includes the .NET runtime and libraries and the application and all dependencies.  
  * Pros:
    * It can be run on a machine that _doesn't_ have the .NET runtime installed
  * Cons:
    * It _must_ be compiled for a specific platform, so the compiled executable is not cross-platform
    * Larger deployable artifact
1. _framework-dependent_ - an app that does not include the .NET runtime and libraries
  * Pros: 
    * Can be compiled and deployed as a cross-platform DLL
    * Smaller deployable artifact
  * Cons:
    * The dotnet runtime for the target platform must already be installed on the machine

There are other publishing and deployment options:

* [Single file deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file) (all dependencies are bundled into a single file)
* [Ready To Run](https://docs.microsoft.com/en-us/dotnet/core/deploying/ready-to-run) - includes a level of pre-compilation to reduce the amount of work done by the JIT (Just in time) compiler
* [Trimmed deployment](https://docs.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained) (applicable to self-contained) - reduces deployment artifact size by removing unused parts of the .NET runtime and libraries

This ADR is to explore the trade-offs in the options above and determine a build, publishing and distribution strategy for the .NET executable that is the Contextive Language Server.

### VS Code Extension Publishing Options

By default, VS Code Extensions are not platform specific.  As they generally run in a Node.js environment provided by VS Code there is no need for platform-specific considerations.  However, the VS Code [publishing model](https://code.visualstudio.com/api/working-with-extensions/publishing-extension) does support [platform-specific extensions](https://code.visualstudio.com/api/working-with-extensions/publishing-extension#platformspecific-extensions) often used for the use case of native node modules.

In our case, as the extension needs to include or obtain the language server, if the included language server is platform-specific then the extension itself must be platform-specific.

## Decision Drivers

* Contextive needs to support a variety of platforms (windows, linux, mac, webassembly)
* Contextive users may not have the correct version of the .NET runtime installed

## Considered Options

The two main options for Language Server distribution are:

1. Embed the language server inside the VS Code extension package
2. Download the language server on extension startup (This is the approach taken by the [Omnisharp package](https://github.com/OmniSharp/omnisharp-vscode))

Either way, the Language Server can be published for distribution using either of the following styles, as discussed in the Context section above:

1. _framework-dependent_
2. _self-contained_

Combining both options, the following table outlines the 4 options:

| Options        | Option 1<br/>Include Language Server | Option 2<br/>Download Language Server |
| - | :-: | :-: |
| **framework-dependent** | 1.a | 2.a |
| **self-contained** | 1.b | 2.b |

## Decision Outcome

Chosen option: Option 1.b - the VS Code Extension includes the language server, and the language server is compiled as a self-contained artifact.

This was selected to provide the best user experience in terms of startup time and simplicity of installation/setup.

### Positive Consequences

* Simpler implementation - no need to host/download/install the language server on startup
* Faster startup time - no delay from the download/install
* Less risk of version incompatibilities between extension and language server

### Negative Consequences

* The Language Server _must_ be compiled with a target platform
* Since the extension includes a target platform-specific binary, the extension itself must be distributed as a platform-specific extension

## Pros and Cons of the Options

### Option 1.a - Included, framework-dependent

* Good, because faster startup as no need to download the language server
* Good, because smaller language server artifact
* Good, because no need to explicitly maintain platform-specific artifacts
* Bad, because the user must either already have the correct .NET runtime version installed, OR the user must wait for the .NET runtime to be downloaded and installed, defeating the point of including the server

### Option 1.b - Included, self-contained

Pros & cons as discussed above.

### Option 2.a - Downloaded, framework-dependent

* Good, no need to build and maintain platform specific versions of either the extension or the language server
  * The language server will work with any platform that the .NET runtime ever supports
* Bad, because the user has to wait for the language server & framework to be downloaded on startup

### Option 2.b - Downloaded, self-contained

* Good, because even though the language server must be platform specific, the VS Code extension does not have to be platform specific
* Bad, because the user has to wait for the language server & framework to be downloaded on startup
* Bad, because as it's self-contained, it must be platform-specific compilation so we still need to build and maintain for supported platforms