module Contextive.LanguageServer.Tests.Helpers.PathExtensions

type System.IO.Path with
    static member OSSafeCompare (p1: string) (p2: string) =
        if System.OperatingSystem.IsWindows() then
            p1.Equals(p2, System.StringComparison.OrdinalIgnoreCase)
        else
            p1 = p2
