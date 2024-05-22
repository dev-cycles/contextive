module Contextive.VsCodeExtension.Waiter

open Fable.Core

let waitForTimeout timeoutMs (condition: unit -> JS.Promise<bool>) =
    promise {
        let endTime = System.DateTime.Now.AddMilliseconds(timeoutMs)

        let mutable conditionResult = false

        while (not conditionResult && System.DateTime.Now < endTime) do
            let! result = condition ()
            conditionResult <- result
            do! Promise.sleep 100

        if (not conditionResult) then
            failwith $"Condition still untrue after {timeoutMs}ms"
    }

let waitFor = waitForTimeout 30000