namespace Contextive.VsCodeExtension

open Fable.Core
open Fable.Import.VSCode.Vscode

type HoverContent = U2<MarkdownString, U2<string, {| language: string; value: string |}>>