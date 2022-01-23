module Contextive.LanguageServer.Completion

open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private completionList labels =
    CompletionList(labels |> Seq.map (fun l -> CompletionItem(Label=l)), isIncomplete=true)

let private termMatches _ = true
let private termToString (caseTemplate:string option) (t:Definitions.Term) = 
    match caseTemplate with
    | None -> t.Name
    | Some(ct) ->
        if ct.Length > 1 && ct |> Seq.forall (System.Char.IsUpper) then
            t.Name.ToUpper()
        elif System.Char.IsUpper(ct.FirstOrDefault()) then
            t.Name.Substring(0,1).ToUpper() + t.Name.Substring(1)
        elif System.Char.IsLower(ct.FirstOrDefault()) then
            t.Name.ToLower()
        else
            t.Name

let handler (termFinder: Definitions.Finder) (wordGetter: TextDocument.WordGetter) (p:CompletionParams) (hc:CompletionCapability) _ =
    async {
        let caseTemplate = 
            match p.TextDocument with
            | null -> None
            | _ -> wordGetter (p.TextDocument.Uri.ToUri()) p.Position
        let termToStringWithCase = termToString caseTemplate
        let! matches = termFinder termMatches
        let labels = matches |> Seq.map termToStringWithCase
        return completionList labels
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)