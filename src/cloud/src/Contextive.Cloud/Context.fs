module Contextive.Cloud.Context

open Constructs

let isLocal (construct: Construct) : bool =
    match construct.Node.TryGetContext("local") with
    | null -> false
    | _ -> true
