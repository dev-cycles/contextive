module Contextive.Cloud.Tests.SynthTest

open Expecto
open Swensen.Unquote
open Contextive.Cloud
open Amazon.CDK
open Amazon.CDK.Assertions

[<Tests>]
let synthTest = testList "Contextive Cloud Stack Tests" [
    testCase "Can Synth Stack" <| fun () ->
        let app = App(null)

        let stack = ContextiveCloudStack(app, "TestStack", StackProps())

        let template = Template.FromStack(stack)

        test <@ template <> null @>
]
