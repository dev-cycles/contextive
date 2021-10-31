module Tests
open Xunit
open System
open System.Collections.Generic
open System.Diagnostics

let shellEx program args writeToStdIn = async {
    let startInfo = ProcessStartInfo()
    startInfo.FileName  <- program
    startInfo.Arguments <- args
    startInfo.UseShellExecute <- false

    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardInput  <- true

    let proc = new Process()
    proc.EnableRaisingEvents <- true

    let outputLines = List<string>()
    proc.OutputDataReceived.AddHandler(
        DataReceivedEventHandler(
            (fun _ args -> 
                printfn "%A" args.Data
                outputLines.Add(args.Data))
        )
    )

    proc.StartInfo <- startInfo
    printfn "Starting process..."
    proc.Start() |> ignore
    proc.BeginOutputReadLine()

    // Now we can write to the program
    do! writeToStdIn proc.StandardInput

    let sleep = async {
        do! Async.Sleep 15000
        proc.Kill()
        return None
    }
    let waitForExit = async {
        proc.WaitForExit()
        return Some proc.ExitCode
    }
    let! exitCode =
        seq {sleep; waitForExit}
        |> Async.Choice

    printf "Process exited."
    return (exitCode, outputLines)
}

let programToTest = "/workspaces/ubictionary/src/Ubictionary.LanguageServer/bin/Debug/net5.0/Ubictionary.LanguageServer"
//let programToTest = "/workspaces/ubictionary/tmp/csharp-language-server-protocol/sample/SampleServer/bin/Debug/netcoreapp3.1/linux-x64/SampleServer"

let getPayloadForMsg msg =
    $"Content-Length: {String.length msg}\r\n\r\n{msg}"

let waitThenInitialize (writer:IO.StreamWriter) = async {
    do! Async.Sleep 1000
    getPayloadForMsg """{
	"jsonrpc": "2.0",
	"id": 2000,
	"method": "initialize",
	"params": {
		"processId": 1,
		"capabilities": {  }
	}
}""" |> writer.Write
    do! Async.Sleep 10000
    getPayloadForMsg """{
	"jsonrpc": "2.0",
	"id": 2001,
	"method": "exit",
}""" |> writer.Write
}

[<Fact>]
let ``Can Start Language Server`` () =
    //waitForDebugger() |> Async.RunSynchronously
    let (exitCode, outputLines) = shellEx programToTest "" waitThenInitialize |> Async.RunSynchronously
    printfn "%A" exitCode
    Assert.Equal("""{"jsonrpc":"2.0","id":2000,"result":{"capabilities":{"experimental":{},"workspace":{"workspaceFolders":{"changeNotifications":false},"fileOperations":{}}},"serverInfo":{"name":"Ubictionary.LanguageServer","version":"1.0.0"}}}""", outputLines.[2])
    Assert.Equal(Some 0, exitCode)
