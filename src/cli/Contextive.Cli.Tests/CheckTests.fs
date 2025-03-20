module Contextive.Cli.Tests.CheckTests

open Expecto

let t = Swensen.Unquote.Assertions.test

[<Tests>]
let checkTests =
    testList "Cli.Check Tests" [ test "one test" { t <@ Contextive.Cli.Check.check = true @> } ]
