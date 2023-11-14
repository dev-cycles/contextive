module Contextive.LanguageServer.Survey

open OmniSharp.Extensions.LanguageServer.Protocol.Server
open OmniSharp.Extensions.LanguageServer.Protocol.Models
open OmniSharp.Extensions.LanguageServer.Protocol.Window
open System.IO
open System.Threading
open System.Threading.Tasks

let private showSurveyPrompt (s: ILanguageServer) (cancellationToken: CancellationToken) =
    task {
        let latchFile = Path.Combine(System.AppContext.BaseDirectory, "survey-prompted.txt")

        if not <| File.Exists(latchFile) then

            let goToSurveyAction = "OK, I'll help!"
            let surveyUri = "https://forms.gle/3pJSUYmLHv5RQ1m1A"

            let surveyPrompt =
                ShowMessageRequestParams(
                    Message =
                        "👋 Please help Contextive by doing a brief survey! Click \"OK, I'll help!\" to be taken to a Google Forms survey. 🙏 to everyone who has responded to the survey so far - your thoughts are invaluable 🧠!",
                    Type = MessageType.Info,
                    Actions =
                        Container(
                            [ MessageActionItem(Title = goToSurveyAction)
                              MessageActionItem(Title = "No (and don't bother me again)") ]
                        )
                )

            let! response = s.Window.ShowMessageRequest(surveyPrompt, cancellationToken)

            if response <> null then
                File.Create(latchFile).Close()

                if response.Title = goToSurveyAction then
                    let! res =
                        s.Window.ShowDocument(ShowDocumentParams(Uri = surveyUri, External = true), cancellationToken)

                    ()
    }
    :> Task

let private nonBlockingShowSurveyPrompt s c = Task.Run(fun _ -> showSurveyPrompt s c)

let public onStartupShowSurveyPrompt =
    OnLanguageServerStartedDelegate(nonBlockingShowSurveyPrompt)
