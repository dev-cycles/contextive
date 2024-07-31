module Contextive.Core.Tests.DefinitionsTests

open Contextive.Core.File
open Contextive.Core.Definitions
open Expecto
open Swensen.Unquote

let unwrap = Result.defaultValue { Contexts = null }

let unwrapError =
    function
    | Ok(o) -> null
    | Error(msg) -> msg

[<Tests>]
let definitionsTests =
    testList
        "Core.Definitions Tests"
        [

          testCase "Term has default"
          <| fun () ->
              let term = Term.Default
              test <@ term.Name = "" @>
              test <@ term.Definition = None @>
              test <@ term.Examples = null @>

          testCase "Context has default"
          <| fun () ->
              let context = Context.Default
              test <@ context.Name = "" @>
              test <@ context.DomainVisionStatement = "" @>
              test <@ context.Paths.Count = 0 @>
              test <@ context.Terms.Count = 0 @>

          testCase "Definitions has default"
          <| fun () ->
              let definitions = Definitions.Default
              test <@ definitions.Contexts.Count = 0 @>

          testCase "Context with new terms has new terms"
          <| fun () ->
              let context = seq { Term.Default } |> Context.defaultWithTerms
              test <@ context.Terms.Count = 1 @>
              test <@ context.Terms[0] = Term.Default @>

          testList
              "Successful Parsing"
              [

                testCase "Minimal parse has parse defaults"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - terms:
    - name: firstTerm
"""

                    test <@ definitions.Contexts.Count = 1 @>
                    let context = Seq.head definitions.Contexts
                    test <@ context.Name = null @>
                    test <@ context.DomainVisionStatement = null @>
                    test <@ context.Paths = null @>
                    test <@ context.Terms.Count = 1 @>
                    let term = context.Terms[0]
                    test <@ term.Definition = None @>
                    test <@ term.Examples = null @>

                testCase "Can Parse Term Name"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - terms:
    - name: firstTerm
"""

                    test <@ definitions.Contexts[0].Terms[0].Name = "firstTerm" @>

                testCase "Can Parse Term Aliases"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - terms:
    - name: firstTerm
      aliases: 
      - "aliasOfFirstTerm"
      - "secondAlias"
"""

                    let aliases = definitions.Contexts[0].Terms[0].Aliases
                    test <@ aliases.Count = 2 @>
                    test <@ List.ofSeq <| aliases = [ "aliasOfFirstTerm"; "secondAlias" ] @>


                testCase "Can Parse Term Definition"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - terms:
    - name: firstTerm
      definition: Some definition
"""

                    test <@ definitions.Contexts[0].Terms[0].Definition = Some "Some definition" @>

                testCase "Can Parse Term Examples"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - terms:
    - name: firstTerm
      examples: 
        - "Example 1"
        - "Example 2"
"""

                    let examples = definitions.Contexts[0].Terms[0].Examples
                    test <@ examples.Count = 2 @>
                    test <@ List.ofSeq <| examples = [ "Example 1"; "Example 2" ] @>

                testCase "Can Parse Context Name"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - name: The Context
"""

                    let context = definitions.Contexts[0]
                    test <@ context.Name = "The Context" @>

                testCase "Can Parse Context Domain Vision Statement"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - domainVisionStatement: The Domain Vision Statement
"""

                    let context = definitions.Contexts[0]
                    test <@ context.DomainVisionStatement = "The Domain Vision Statement" @>

                testCase "Empty List parses as empty list instead of null"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - terms:
"""

                    let context = definitions.Contexts[0]
                    test <@ context.Terms.Count = 0 @>


                testCase "Can Parse Context Paths"
                <| fun () ->
                    let definitions =
                        unwrap
                        <| deserialize
                            """
contexts:
  - paths:
    - "path1"
    - "path2"
"""

                    let context = definitions.Contexts[0]
                    test <@ List.ofSeq <| context.Paths = [ "path1"; "path2" ] @>

                testList
                    "Multi-line context name"
                    [

                      let testContextName name yml expectedName =
                          testCase name
                          <| fun () ->
                              let definitions = unwrap <| deserialize yml

                              let context = definitions.Contexts[0]
                              test <@ context.Name.ReplaceLineEndings() = expectedName @>

                      testContextName
                          "Folded, Clip"
                          """
contexts:
  - name: >
      this name has a single
      newline at the end

"""
                          ("this name has a single newline at the end" + System.Environment.NewLine)

                      testContextName
                          "Folded, Chomp"
                          """
contexts:
  - name: >-
      this name has no
      newline at the end

"""
                          "this name has no newline at the end"

                      testContextName
                          "Folded, Keep"
                          """
contexts:
  - name: >+
      this name has two
      newlines at the end

"""
                          ("this name has two newlines at the end"
                           + System.Environment.NewLine
                           + System.Environment.NewLine)

                      testContextName
                          "Literal, Clip"
                          """
contexts:
  - name: |
      this name has a single
      newline in the middle and one at the end

"""
                          ("this name has a single"
                           + System.Environment.NewLine
                           + "newline in the middle and one at the end"
                           + System.Environment.NewLine)

                      testContextName
                          "Literal, Chomp"
                          """
contexts:
  - name: |-
      this name has a single
      newline in the middle and none at the end

"""
                          ("this name has a single"
                           + System.Environment.NewLine
                           + "newline in the middle and none at the end")

                      testContextName
                          "Literal, Keep"
                          """
contexts:
  - name: |+
      this name has a single
      newline in the middle and two at the end

"""
                          ("this name has a single"
                           + System.Environment.NewLine
                           + "newline in the middle and two at the end"
                           + System.Environment.NewLine
                           + System.Environment.NewLine) ] ]

          testList
              "Parsing Errors"
              [

                testCase "Error when file is empty"
                <| fun () ->
                    let result = deserialize ""
                    test <@ result = Error(ParsingError("Definitions file is empty.")) @>

                testCase "Error with unexpected key"
                <| fun () ->
                    let result =
                        deserialize
                            """
unknown:
"""

                    test
                        <@
                            result = Error(
                                ParsingError(
                                    "Object starting line 2, column 1 - Property 'unknown' not found on type 'Contextive.Core.Definitions+Definitions'."
                                )
                            )
                        @>

                testCase "Error with extra colon typo"
                <| fun () ->
                    let result =
                        deserialize
                            """
contexts:
  - terms:
    - name: invalidTerm
      definition: : A definition with an extra colon is invalid yml
"""

                    test
                        <@
                            result = Error(
                                ParsingError(
                                    "Object starting line 5, column 19 - Mapping values are not allowed in this context."
                                )
                            )
                        @> ]
          testList
              "ValidationErrors"
              [ testCase "Term name is required"
                <| fun () ->
                    let result =
                        deserialize
                            """
contexts:
  - terms:
    - name:
"""

                    test <@ result = Error(ValidationError("The Name field is required. See line 4, column 7.")) @> ] ]
