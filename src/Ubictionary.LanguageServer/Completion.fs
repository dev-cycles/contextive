module Ubictionary.LanguageServer.Completion

open System.Threading.Tasks
open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private completionList labels =
    CompletionList(labels |> Seq.map (fun l -> CompletionItem(Label=l)))

let private termMatches _ = true
let private termToString (caseTemplate:string option) (t:Definitions.Term) = 
    match caseTemplate with
    | None -> seq {t.Name}
    | Some(ct) ->
        if ct.Length > 1 && ct |> Seq.forall (System.Char.IsUpper) then
            seq {t.Name.ToUpper()}
        elif System.Char.IsUpper(ct.LastOrDefault()) then
            seq {t.Name.Substring(0,1).ToUpper() + t.Name.Substring(1); t.Name.ToUpper()}
        else
            seq {t.Name}

let handler (termFinder: Definitions.Finder) (wordGetter: TextDocument.WordGetter) (p:CompletionParams) (hc:CompletionCapability) _ = 
    let caseTemplate = 
        match p.TextDocument with
        | null -> None
        | _ -> wordGetter (p.TextDocument.Uri.ToUri()) p.Position
    let termToStringWithCase = termToString caseTemplate
    let labels = termFinder termMatches |> Seq.collect termToStringWithCase
    Task.FromResult(completionList labels)

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)