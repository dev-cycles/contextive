namespace Fable.Mocha

module Expect =

    let seqEqual (expected: seq<'a>) (actual: seq<'a>) (msg: string) : unit =
        let comparison = Seq.compareWith compare expected actual

        match comparison with
        | 0 -> Expect.equal comparison 0 msg
        | _ ->
            let prefix = "\n\t"

            let format sequence =
                "seq {"
                + (prefix + (sequence |> Seq.map (sprintf "%A") |> String.concat prefix))
                + "\n}"

            let expectedList = format expected
            let actualList = format actual
            Expect.equal actualList expectedList msg
