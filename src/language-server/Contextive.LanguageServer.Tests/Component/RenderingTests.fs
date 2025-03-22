module Contextive.LanguageServer.Tests.Component.RenderingTests

open Expecto
open Swensen.Unquote
open Contextive.LanguageServer
open Contextive.Core.GlossaryFile
open Contextive.LanguageServer.Tests.Helpers

[<Tests>]
let tests =
    testList
        "LanguageServer.Rendering Tests"
        [

          let testTermRender (terms: Term list, expectedHover: string) =
              testCase $"Terms: {terms |> List.map (_.Name)}"
              <| fun _ ->
                  let contexts = seq { Context.defaultWithTerms terms }

                  let rendering = Rendering.renderContexts contexts

                  test <@ rendering.Value.ReplaceLineEndings() = expectedHover @>


          [ [ { Term.Default with
                  Name = "firstTerm"
                  Definition = Some "The first term in our definitions list" } ],
            "### 📗 `firstTerm`

> 📝 The first term in our definitions list"

            [ { Term.Default with
                  Name = "termWithAlias"
                  Aliases = ResizeArray [ "aliasOfTerm" ] } ],
            """### 📗 `termWithAlias`

> 📝 _undefined_  
_Aliases_: _aliasOfTerm_"""

            [ { Term.Default with
                  Name = "termWithAliases"
                  Aliases = ResizeArray [ "aliasOfTerm"; "anotherAlias" ] } ],
            """### 📗 `termWithAliases`

> 📝 _undefined_  
_Aliases_: _aliasOfTerm_, _anotherAlias_"""

            [ { Term.Default with
                  Name = "SecondTerm" } ],
            "### 📗 `SecondTerm`

> 📝 _undefined_"

            [ { Term.Default with
                  Name = "ThirdTerm"
                  Examples = ResizeArray [ "Do a thing" ] } ],
            "\
### 📗 `ThirdTerm`

> 📝 _undefined_

💬 \"Do a thing\""

            [ { Term.Default with
                  Name = "ThirdTermWithTrailingNewLineInUsage"
                  Examples = ResizeArray [ "Do a thing" + System.Environment.NewLine ] } ],
            "\
### 📗 `ThirdTermWithTrailingNewLineInUsage`

> 📝 _undefined_

💬 \"Do a thing\""

            [ { Term.Default with
                  Name = "ThirdTermWithTrailingWhitespaceInUsage"
                  Examples = ResizeArray [ "Do a thing " ] } ],
            "\
### 📗 `ThirdTermWithTrailingWhitespaceInUsage`

> 📝 _undefined_

💬 \"Do a thing\""

            [ { Term.Default with
                  Name = "ThirdTermWithLeadingWhitespaceInUsage"
                  Examples = ResizeArray [ " Do a thing" ] } ],
            "\
### 📗 `ThirdTermWithLeadingWhitespaceInUsage`

> 📝 _undefined_

💬 \"Do a thing\""

            [ { Term.Default with
                  Name = "Metadata"
                  Meta = dict [ "key", "value" ] } ],
            "\
### 📗 `Metadata`

> 📝 _undefined_

key value"

            [ { Term.Default with
                  Name = "Metadata with multiple keys"
                  Meta = dict [ "key", "value"; "key2", "value2" ] } ],
            "\
### 📗 `Metadata with multiple keys`

> 📝 _undefined_

key value

key2 value2"

            [ { Term.Default with Name = "Second" }; { Term.Default with Name = "Term" } ],
            "\
### 📗 `Second`

> 📝 _undefined_

***

### 📗 `Term`

> 📝 _undefined_"

            [ { Term.Default with
                  Name = "First"
                  Examples = ResizeArray [ "Do a thing" ] }
              { Term.Default with Name = "Term" } ],
            "\
### 📗 `First`

> 📝 _undefined_

💬 \"Do a thing\"

***

### 📗 `Term`

> 📝 _undefined_"

            [ { Term.Default with
                  Name = "TermWithExamples"
                  Examples = ResizeArray [ "Do a thing" ]
                  Meta = dict [ "key", "value" ] }
              { Term.Default with
                  Name = "AnotherTermWithExamples"
                  Examples = ResizeArray [ "Do something else"; "Do the third thing" ] } ],
            "\
### 📗 `TermWithExamples`

> 📝 _undefined_

💬 \"Do a thing\"

key value

***

### 📗 `AnotherTermWithExamples`

> 📝 _undefined_

💬 \"Do something else\"

💬 \"Do the third thing\"" ]
          |> List.map testTermRender
          |> testList "Render Terms"

          testCase "Render fully defined Context"
          <| fun _ ->
              let contexts =
                  seq {
                      { Context.Default with
                          Name = "TestContext"
                          DomainVisionStatement = "supporting the test"
                          Meta = dict [ "owner", "Team A" ] }
                  }
                  |> GlossaryHelper.allContextsWithTermNames [ "term" ]

              let rendering = Rendering.renderContexts contexts

              let expectedHover =
                  "\
## 💠 TestContext Context

_Vision: supporting the test_

owner Team A

***

### 📗 `term`

> 📝 _undefined_"

              test <@ rendering.Value.ReplaceLineEndings() = expectedHover @>

          testCase "Render multiple Contexts"
          <| fun _ ->
              let contexts =
                  seq {
                      { Context.Default with Name = "Test" }
                      { Context.Default with Name = "Other" }
                  }
                  |> GlossaryHelper.allContextsWithTermNames [ "term" ]

              let rendering = Rendering.renderContexts contexts

              let expectedHover =
                  "\
## 💠 Test Context

***

### 📗 `term`

> 📝 _undefined_

***

## 💠 Other Context

***

### 📗 `term`

> 📝 _undefined_"

              test <@ rendering.Value.ReplaceLineEndings() = expectedHover @>

          testCase "Render vision statement with newline at the end"
          <| fun _ ->
              let contexts =
                  seq {
                      { Context.Default with
                          Name = "Test"
                          DomainVisionStatement = "vision statement should still be italic" + System.Environment.NewLine }
                  }
                  |> GlossaryHelper.allContextsWithTermNames [ "term" ]

              let rendering = Rendering.renderContexts contexts

              let expectedHover =
                  "\
## 💠 Test Context

_Vision: vision statement should still be italic_

***

### 📗 `term`

> 📝 _undefined_"

              test <@ rendering.Value.ReplaceLineEndings() = expectedHover @>

          testCase "Render vision statement with whitespace at the end"
          <| fun _ ->
              let contexts =
                  seq {
                      { Context.Default with
                          Name = "Test"
                          DomainVisionStatement = "vision statement should still be italic " }
                  }
                  |> GlossaryHelper.allContextsWithTermNames [ "term" ]

              let rendering = Rendering.renderContexts contexts

              let expectedHover =
                  "\
## 💠 Test Context

_Vision: vision statement should still be italic_

***

### 📗 `term`

> 📝 _undefined_"

              test <@ rendering.Value.ReplaceLineEndings() = expectedHover @> ]
