module Contextive.Core.File

type FileError =
    | PathInvalid of string
    | FileNotFound
    | NotYetLoaded
    | ParsingError of string

type File =
    { AbsolutePath: string
      Contents: Result<string, FileError> }
    
let fileErrorMessage =
    function
    | PathInvalid(m) -> $"Invalid Path: {m}"
    | ParsingError(m) -> $"Parsing Error: {m}"
    | FileNotFound -> "Definitions file not found."
    | NotYetLoaded -> "Should Not Be Reached"
