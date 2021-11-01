# Language & Framework Selection 

* Status: proposed
* Deciders: Chris Simon
* Date: 2021-10-23

## Decision Summary

Use F# for the Language Server, F#/Fable for the VSCode extension & F# for the Visual Studio 2019/2022 extension.

## Context and Problem Statement

Following ADR0001, Ubictionary will use the Language Server Protocol as the primary architectural pattern.  This ADR is to determine the languages/frameworks that will be used to implement the various components.

The components are:
* The Language Server, owning planned Ubictionary features such as:
  * Providing auto-complete suggestions
  * Information from the Ubictionary to be displayed on hover, such as:
    * Definitions of Ubiquitous Language terms
    * Links to related terms and code usage
  * Automatic support for term variations (e.g. tense, pluralization)
  * Managing relationships between terms (e.g. entities and associated commands or events)
* The IDE-specific Extensions that owns:
  * Internationalization
  * UI Interactions for managing the Ubictionary settings and data (e.g. ubiquitous language term definitions)
  * Interactions with the Language Server via the Language Server Protocol

## Decision Drivers

* Ubictionary will initially have a small team, so using the same language for multiple components will reduce cognitive load
* Ability to share libraries, such as service contract/dto class definitions should reduce risk of errors
* Initial contributor has most experience with .NET
* The Language Server needs to run on the IDE's Operating System
* The IDE Extensions must be written in a language that supports the IDE extension/plugin execution environment
* While the MVP does not have any online components, the overall project may one day incorporate an online service, so a language/framework that supports web app and API development is beneficial

Factors not specifically considered:
* While all languages have inherent strengths and weaknesses, these have not been considered in this evaluation due to the number of languages and lack of clarity at this point over which strengths and weaknesses are most relevant for the development of a Ubictionary type project.
* As a result, the primary consideration is support for the relevant and priority execution environments
* Inherent language strength/weakness concerns may be considered if there is a need to narrow down two viable candidates, or after MVP when the project needs are more well understood

## Considered Options

To determine the language/framework options, we need to consider the target IDEs and their execution environments

### IDEs

| IDE | OS | Plugin Execution Environment |
| - | - | - |
| **Initial** |
| [VSCode](https://code.visualstudio.com/api/language-extensions/language-server-extension-guide) | Cross-Platform | JavaScript 
| [VSCode Web](https://code.visualstudio.com/api/extension-guides/web-extensions#language-server-protocol-in-web-extensions) | Browser | JavaScript |
| **Later** |
| [Visual Studio 2019/2022](https://docs.microsoft.com/en-us/visualstudio/extensibility/starting-to-develop-visual-studio-extensions?view=vs-2019) | Windows | .NET, C++ |
| [Eclipse](https://projects.eclipse.org/projects/technology.lsp4e) | Cross-Platform | JVM |
| [NetBeans](https://netbeans.apache.org/tutorials/nbm-google.html) | Cross-Platform | JVM |
| [JetBrains IDEs](https://plugins.jetbrains.com/docs/intellij/getting-started.html) | Cross-Platform | JVM |
| [vim](https://learnvimscriptthehardway.stevelosh.com/) | Cross-Platform | vimscript |
| [emacs](https://spin.atomicobject.com/2016/05/27/write-emacs-package/) | Cross-Platform | Lisp |

* **Note:** Visual Studio for Mac was evaluated, but plugin development is [no longer supported](https://docs.microsoft.com/en-us/visualstudio/mac/migrate-extensions?view=vsmac-2019).

### Execution Environments and Supported Languages

| Execution Environment | Languages |
| - | - |
| Cross-Platform | All considered languages can be cross-compiled |
| Browser | (See JavaScript) |
| [JavaScript](https://www.ecma-international.org/publications-and-standards/standards/ecma-262/) | [TypeScript](https://www.typescriptlang.org/), [F#/Fable](https://fable.io), [Scala.js](https://www.scala-js.org/), [ClojureScript](https://clojurescript.org/), [Kotlin/JS](https://kotlinlang.org/docs/js-overview.html), [GrooScript](https://www.grooscript.org/), [WebAssembly](https://webassembly.org/) |
| [.NET](https://dotnet.microsoft.com/) | [F#](https://dotnet.microsoft.com/languages/fsharp), [C#](https://dotnet.microsoft.com/languages/csharp), [VB.NET](https://dotnet.microsoft.com/languages#visual-basic) |
| [JVM](https://www.oracle.com/java/technologies/java-se-glance.html)  | [Java](https://dev.java/), [Kotlin](https://kotlinlang.org), [Scala](https://www.scala-lang.org/), [Clojure](https://clojure.org/), [Groovy](https://groovy-lang.org/) |

## Decision Outcome

Chosen option: "F#", because it appears to offer the best combination of an at least partially trodden path for non-JS or TS VSCode extension development, as well as the possibility of having a common language with at least one other IDE (Visual Studio 2019/2022).

F# will be used for the MVP build and re-evaluated, at which point the status of this ADR should be updated to `accepted` for further development of Ubictionary.

### Considerations

* All considered languages are cross-platform
* Given VSCode is an initial target environment, we need to narrow the range to those which can compile to JavaScript or WebAssembly
  * Noting that, while many languages have WebAssembly compilation support, using WebAssembly for either VSCode or VSCode Web extension development is [experimental](https://github.com/Microsoft/vscode/issues/65559) at [best](https://john-millikin.com/extending-vscode-with-webassembly)


### Positive Consequences

* To be confirmed after MVP Build

### Negative Consequences

* To be confirmed after MVP Build

## Pros and Cons of the Options

### F#

* Good, because F# with Fable has at least a [handful](https://github.com/inosik/fable-vscode-rollup-sample) of [examples](https://github.com/LambdaFactory/fable-vscode-demo) for building VS Code Extensions
* Good, because it should be possible to build the Visual Studio 2019/2022 extension in F#
* Good, because F# has great support for web app and API development in case that becomes relevant in the future
* Bad, because Fable is primarily oriented at web development, so VS Code extension development isn't that well documented or supported
* Bad, because Visual Studio 2019/2022 extension development documentation is primarily targeted at C#, C++ and VB.NET

### JavaScript / TypeScript

* Good, because all VSCode & VS Code Web extension development tutorials assume one of these languages
* Bad, because no other IDE uses either language for extension/plugin development

### A JVM Language that supports JavaScript Compilation

(e.g. Clojure & ClojureScript, Scala & Scala.Js, Kotlin & Kotlin/JS)

* Good, because it _should_ be possible to build the Language Server, the VSCode extension AND all JVM IDE extensions in the same language
* Bad, because there are limited examples of using these languages to build VSCode extensions
* Bad, because even JVM IDEs (e.g. Eclipse) have [limited](https://stackoverflow.com/questions/58784058/using-kotlin-to-develope-an-eclipse-plugin) support for JVM languages other than Java