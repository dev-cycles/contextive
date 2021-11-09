module Ubictionary.LanguageServer.Definitions

type Term =
    {
        Slug: string
    }


let mutable private ubictionaryPath : string option = None

let load (path:string option) =
    ubictionaryPath <- path
    ()

let find (matcher: Term -> bool) (transformer: Term -> 'a) : 'a seq =
    let terms =
        match ubictionaryPath  with
        | None -> seq []
        | _ -> seq [{Slug="firstTerm"};{Slug="secondTerm"};{Slug="thirdTerm"}]
    terms |> Seq.map transformer