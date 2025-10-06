module Contextive.Core.File

type ConfigurationSource =
    | Default
    | Configured
    | Discovered
    | Imported

type PathConfiguration =
    { Path: string
      Source: ConfigurationSource }

let configuredPath p = { Path = p; Source = Configured }

type FileError =
    | PathInvalid of string
    | FileNotFound of ConfigurationSource
    | NotYetLoaded
    | ReadingError of string
    | ParsingError of string
    | ValidationError of string

let fileErrorMessage =
    function
    | PathInvalid m -> $"Invalid Path: {m}"
    | ParsingError m -> $"Parsing Error: {m}"
    | ValidationError m -> $"Validation Error: {m}"
    | ReadingError m -> $"Unable to read from source: {m}"
    | FileNotFound Default -> "Default glossary file not found."
    | FileNotFound Configured -> "Glossary file not found."
    | FileNotFound Discovered -> "Watched Glossary file not found."
    | FileNotFound Imported -> "Imported Glossary file not found."
    | NotYetLoaded -> "Should Not Be Reached."
