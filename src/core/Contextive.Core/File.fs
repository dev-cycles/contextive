module Contextive.Core.File

type File =
    { AbsolutePath: string
      Contents: Result<string, string> }
