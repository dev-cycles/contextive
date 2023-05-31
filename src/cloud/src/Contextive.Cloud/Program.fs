open Contextive.Cloud

open Amazon.CDK

[<EntryPoint>]
let main _ =
    let app = App(null)

    let contextiveCloud = ContextiveCloudStack(app, "ContextiveCloud", StackProps())
    Tags.Of(contextiveCloud).Add("app-name", "ContextiveCloud") |> ignore
    app.Synth() |> ignore
    0
