namespace Ubictionary.VsCodeExtension

open Fable.Core.JsInterop
open Fable.Import.VSCode.Vscode

module Extension =
    let testFn x = x * 2

    let activate (context: ExtensionContext) = promise {
        printf $"Extension is activated from {context.extensionPath} :)"
        window.showInformationMessage($"Extension is activated from {context.extensionPath} :)") |> ignore
        let opt = createEmpty<InputBoxOptions>;
        opt.prompt <- Some "Enter some data"
        let! response = window.showInputBox(opt);
        window.showInformationMessage($"You entered: {response}") |> ignore
    }
