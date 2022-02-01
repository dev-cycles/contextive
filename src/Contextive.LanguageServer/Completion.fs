module Contextive.LanguageServer.Completion

open System.Linq
open OmniSharp.Extensions.LanguageServer.Protocol
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities

let private completionList labels =
    CompletionList(labels |> Seq.map (fun l -> CompletionItem(Label=l)), isIncomplete=true)

let private termFilter _ = true
let private termToString (caseTemplate:TextDocument.WordAndParts option) (t:Definitions.Term) = 
    match caseTemplate with
    | None -> t.Name
    | Some((ct,_)) ->
        if ct.Length > 1 && ct |> Seq.forall (System.Char.IsUpper) then
            t.Name.ToUpper()
        elif System.Char.IsUpper(ct.FirstOrDefault()) then
            t.Name.Substring(0,1).ToUpper() + t.Name.Substring(1)
        elif System.Char.IsLower(ct.FirstOrDefault()) then
            t.Name.ToLower()
        else
            t.Name

let handler (termFinder: Definitions.Finder) (wordsGetter: TextDocument.WordGetter) (p:CompletionParams) (hc:CompletionCapability) _ =
    async {
        let caseTemplate = 
            match p.TextDocument with
            | null -> None
            | _ -> wordsGetter (p.TextDocument.Uri.ToUri()) p.Position |> List.tryHead
        let termToStringWithCase = termToString caseTemplate
        let uri = p.TextDocument.Uri.ToString()
        let! findResult = termFinder uri termFilter
        let labels = findResult
                     |> Definitions.FindResult.allTerms
                     |> Seq.map termToStringWithCase
        return completionList labels
    } |> Async.StartAsTask

let private registrationOptionsProvider (hc:CompletionCapability) (cc:ClientCapabilities)  =
    CompletionRegistrationOptions()

let registrationOptions =
    RegistrationOptionsDelegate<CompletionRegistrationOptions, CompletionCapability>(registrationOptionsProvider)