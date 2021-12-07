open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.Playwright
open Giraffe
open System.Text.Json
open Microsoft.AspNetCore.Builder


[<Struct>]
type TestError =
    { actual: obj
      expected: obj
      message: string
      name: string
      stack: string }

[<Struct>]
type Test =
    { name: string
      passed: bool
      duration: int
      error: TestError voption }

[<Struct>]
type Suite =
    { name: string
      suites: Suite array
      tests: Test array }

[<Struct>]
type ExecutionResult =
    { logs: obj seq
      errors: obj seq
      passed: bool
      testResults: Suite }

type SendMessagePayload =
    { ``type``: string
      sessionId: string option
      testFile: string option
      userAgent: string option
      result: ExecutionResult voption }

[<RequireQualifiedAccess>]
module TestLogger =

    let private logError (error: TestError) =

        printfn $"{error.name}: [Actual: {error.actual} - Expected: {error.expected}]"
        printfn $"\t{error.message}"
        printfn "\t%s" error.stack

    let private logTest (test: Test) =
        printfn $"{test.name} - [Passed: {test.passed} {test.duration}ms]"

        match test.error with
        | ValueNone -> ()
        | ValueSome error -> logError error

    let rec private logSuite (suite: Suite) =
        for suite in suite.suites do
            logSuite suite

        printfn $"Suite: {suite.name}"

        for test in suite.tests do
            logTest test

    let private logExecutionResult (result: ExecutionResult ValueOption) =
        match result with
        | ValueSome result ->
            printfn $"Passed: {result.passed}"
            logSuite result.testResults
        | ValueNone -> ()

    let LogMessage (payload: SendMessagePayload) =
        let agent = defaultArg payload.userAgent ""
        printfn $"Runnning Tests in {agent}:"
        logExecutionResult payload.result


let getStandaloneScripts () =
    Directory.EnumerateFiles("./wwwroot/tests", "*.js")
    |> Seq.map (fun path -> path.Replace("wwwroot/", ""))

let getGroupedScripts () =

    Directory.GetDirectories "./wwwroot/tests/"
    |> Seq.map (fun dir ->
        let children =
            Directory.EnumerateFiles(dir, "*.js")
            |> Seq.map (fun path -> path.Replace("wwwroot/", ""))

        let dirName = Path.GetFileNameWithoutExtension dir
        dirName, children)
    |> Map.ofSeq

let builder = WebApplication.CreateBuilder([||])

builder.Services.AddGiraffe() |> ignore

let app = builder.Build()

let clientHandler _ (ctx: HttpContext) =
    task {
        let! content = File.ReadAllTextAsync "./test.tpl.html"
        let tpl = Scriban.Template.Parse content
        let standalone = getStandaloneScripts ()
        let grouped = getGroupedScripts ()

        let! result =
            tpl.RenderAsync(
                {| standalone = standalone
                   grouped = grouped
                   runtimeConfig =
                    {| testFile = None
                       watch = false
                       debug = true
                       testFrameworkConfig = None |} |}
            )

        return! ctx.WriteHtmlStringAsync(result)
    }

let socketScriptHandler _ (ctx: HttpContext) =
    task {
        ctx.SetContentType("text/javascript")
        return! ctx.WriteFileStreamAsync(false, "./mock.js", None, None)
    }

let clientMessageHandler _ (ctx: HttpContext) =
    task {
        let! msg = JsonSerializer.DeserializeAsync<SendMessagePayload>(ctx.Request.Body)
        TestLogger.LogMessage msg
        return! ctx.WriteJsonAsync({|  |})
    }

let webApp =
    choose [ GET
             >=> choose [ route "/" >=> clientHandler
                          route "/__web-dev-server__web-socket.js"
                          >=> socketScriptHandler ]
             POST
             >=> route "/~nacre~/messages"
             >=> clientMessageHandler ]

app.UseGiraffe webApp
app.UseStaticFiles() |> ignore

task {
    do! Async.Sleep(TimeSpan.FromSeconds(3.))
    let! pl = Playwright.CreateAsync()
    let brOptions = BrowserTypeLaunchOptions()
    brOptions.Devtools <- true
    let! browser = pl.Chromium.LaunchAsync(brOptions)
    let! page = browser.NewPageAsync()
    let! _ = page.GotoAsync "http://localhost:5000?wtr-session-id=1"
    return ()
}
|> Async.AwaitTask
|> Async.StartImmediate

app.Run()
