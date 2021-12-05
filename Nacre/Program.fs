open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.Playwright
open Saturn
open Giraffe

let app =
    let appRouter =
        router {
            get "/test" (htmlFile "./sample.test.html")
            get "/__web-dev-server__web-socket.js" (fun next (ctx: HttpContext) ->
                task {
                    let! content = File.ReadAllTextAsync"./mock.js"
                    ctx.SetHttpHeader("Content-Type", "text/javascript")
                    return! ctx.WriteStringAsync content
                }
            )
        }

    application {
        use_router appRouter
        use_gzip
    }
task {
    do! Async.Sleep(TimeSpan.FromSeconds(3.))
    let! pl = Playwright.CreateAsync()
    let brOptions = BrowserTypeLaunchOptions()
    brOptions.Devtools <- true
    let! browser = pl.Chromium.LaunchAsync(brOptions)
    let! page = browser.NewPageAsync()
    let! _ = page.GotoAsync "http://localhost:5000/test?wtr-session-id=1"
    return ()
}
|> Async.AwaitTask
|> Async.StartImmediate

run app
