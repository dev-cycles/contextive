module Contextive.Core.File

type PathConfiguration = { Path: string; IsDefault: bool }

let configuredPath p = { Path = p; IsDefault = false }

type FileError =
    | Uninitialized
    | PathInvalid of string
    | DefaultFileNotFound
    | FileNotFound
    | NotYetLoaded
    | ParsingError of string
    | ValidationError of string

type File =
    { AbsolutePath: string
      Contents: Result<string, FileError> }

let fileErrorMessage =
    function
    | Uninitialized -> "Attempting to load file before the system is initialized."
    | PathInvalid(m) -> $"Invalid Path: {m}"
    | ParsingError(m) -> $"Parsing Error: {m}"
    | ValidationError(m) -> $"Validation Error: {m}"
    | DefaultFileNotFound -> "Default glossary file not found."
    | FileNotFound -> "Glossary file not found."
    | NotYetLoaded -> "Should Not Be Reached"
