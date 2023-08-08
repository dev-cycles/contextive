module Contextive.VsCodeExtension.Initialize

open Fable.Import.VSCode
open Fable.Import.VSCode.Vscode

let private defaultDefinitions =
    """# Welcome to Contextive!
#
# This initial definitions file illustrates the syntax of the file by providing definitions and examples of the terms
# used in schema of the definitions file.
#
# Hover over some of the defined words, such as `context`, `term`, `definition` and `example` to see Contextive in action.
#
# Update the yaml below to define your specific contexts and definitions, and feel free to use markdown in definitions and examples.
contexts:
  - name: Demo
    domainVisionStatement: To illustrate the usage of the contextive definitions file.
    paths:
    # Globs are supported. Multiple paths may be included. If any match the currently open file, the context will be used.
    - "**" # This is the default path glob if the `paths` key is excluded, and will match all files
    terms:
    - name: context
      definition: A bounded set of definitions within which words have specific meanings.
      examples:
        - In the _Sales_ context, the language focuses on activities associated with selling products.
        - Are you sure you're thinking of the definition from the right context?
    - name: term
      definition: A protected word in a given context.
      examples:
        - _Order_ is a term that is meaningful in the _Sales_ context.
        - What term should we use to describe what happens when a customer buys something from us?
      aliases:
        - word
    - name: alias
      definition: >
        An alternative word for the same concept.
        It might be a legacy term, or an alternative term that is in common use while the ubiquitous language is still being adopted.
      examples:
        - _Product_ is an alias of _Item_ in the Sales context.
    - name: definition
      definition: A short summary defining the meaning of a term in a context.
      examples:
        - "The definition of _Order_ in the _Sales_ context is: \"The set of _Items_ that are being sold as part of this _Order_\"."
        - Can you provide a definition of the term _Order_?
    - name: example
      definition: A sentence illustrating the correct usage of the term in the context.
      examples:
        - Can you give me an example of how to use the term _Order_ in this context?"""

let private fileExists uri =
    promise {
        let! _ = workspace.fs.stat (uri)
        return true
    }
    |> Promise.catch (fun _ -> false)

let initializeHandler (definitionsUri: Uri) =
    promise {
        let! exists = fileExists definitionsUri

        do!
            match exists with
            | true ->
                promise {
                    do! window.showTextDocument (definitionsUri) |> Thenable.Ignore
                    ()
                }
            | false ->
                promise {
                    let edit = vscode.WorkspaceEdit.Create()
                    edit.createFile (definitionsUri)
                    edit.insert (definitionsUri, vscode.Position.Create(0, 0), defaultDefinitions)
                    do! workspace.applyEdit (edit) |> Thenable.Ignore
                    let! document = workspace.openTextDocument (definitionsUri)
                    do! document.save () |> Thenable.Ignore
                    do! window.showTextDocument (definitionsUri) |> Thenable.Ignore
                    ()
                }
    }

let handler (definitionsUriLoader: unit -> Uri option) _ : obj option =
    promise {
        let definitionsUriSetting = definitionsUriLoader ()

        do!
            match definitionsUriSetting with
            | Some definitionsUri -> initializeHandler definitionsUri
            | None -> promise { return () }
    }
    :> obj
    |> Some
