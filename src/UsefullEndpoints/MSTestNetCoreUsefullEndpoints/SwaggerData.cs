using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using static System.Net.WebRequestMethods;

namespace MSTestNetCoreUsefullEndpoints;

[TestClass]
public class SwaggerData : PageTest
{
    private async Task<bool> WaitDefault()
    {
        await Task.Delay(3000);
        return true;
    }
    [TestMethod]
    public async Task TestGetNoParameters()
    {
        var pathVideos = "videos";
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.UsefullEndpoints_AppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();
        // Act
        var httpClient = app.CreateHttpClient("testusefullendpoints");
        await resourceNotificationService.WaitForResourceAsync("testusefullendpoints");
        var baseUrl = httpClient.BaseAddress;
        var response = await httpClient.GetAsync("/swagger/v1/swagger.json");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        await Page.GotoAsync(baseUrl+"swagger/index.html");
        var data= await Page.ScreenshotAsync();
        await System.IO.File.WriteAllBytesAsync("swagger.png", data);
        using var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;
        if (!Directory.Exists(pathVideos))
            Directory.CreateDirectory(pathVideos);
        var openApiDocument = new OpenApiStreamReader().Read(ms, out var diagnostic);
        foreach (var path in openApiDocument.Paths)
        {
            foreach (var op in path.Value.Operations)
            {
                if (op.Key == OperationType.Get)
                {
                    if (op.Value.Parameters.Count == 0)
                    {
                        var url = baseUrl + path.Key;
                        url = url.Replace("//", "/");
                        //await Page.GotoAsync(url);
                        var name = path.Key.Replace("/", "_");
                        string pathVideo = pathVideos + name;
                        var cntVideo = await base.NewContextAsync(new()
                        {
                            RecordVideoDir=pathVideo,
                            
                        });
                        var page = await cntVideo.NewPageAsync();
                        await page.GotoAsync(baseUrl + "swagger/index.html");
                        await page.WaitForLoadStateAsync();
                        await page.ScreenshotAsync();
                        var element = page.GetByText(path.Key, new PageGetByTextOptions()
                        {
                            Exact=true,
                        });
                        await WaitDefault();
                        await element.ClickAsync();
                        var allOperation = page.Locator("[class='opblock-summary opblock-summary-get']"
                            , new PageLocatorOptions()
                            {
                                Has = element
                            }
                            )
                            .Locator("..")
                            //.Locator("..")
                            //.GetByText("Try it out")
                            
                            ;
                        var btnTry = allOperation.GetByText("Try it out");
                        await WaitDefault();
                        await btnTry.HoverAsync();
                        await WaitDefault();
                        await btnTry.HighlightAsync();
                        await WaitDefault();
                        await btnTry.ClickAsync();
                        var btnExec = allOperation.GetByText($"Execute");
                        await WaitDefault();
                        await btnExec.HoverAsync();
                        await WaitDefault();
                        await btnTry.HighlightAsync();
                        await WaitDefault();
                        await btnExec.ClickAsync();
                        //await allOperation.HighlightAsync();

                        //PageScreenshotOptions options = new PageScreenshotOptions();
                        //options.FullPage = true;   
                        //options.Path = name + ".png";
                        //await Page.ScreenshotAsync(options);
                        //await page.Locator(".header").ScreenshotAsync(new() { Path = "screenshot.png" });
                        //await Page.ScreenshotAsync(new()
                        //{
                        //    Path = name + ".png",
                        //    FullPage = true,
                        //});
                        await allOperation.ScrollIntoViewIfNeededAsync();
                        await allOperation.ScreenshotAsync(new LocatorScreenshotOptions()
                        {
                            Path=name+".png"
                        });
                        await WaitDefault();
                        //await element.ClickAsync();
                        //await WaitDefault();
                        await cntVideo.DisposeAsync();
                        var file = Directory.GetFiles(pathVideo).First();
                        System.IO.File.Move(file, Path.Combine(pathVideos,name+ ".webm"));

                        //return;
                    }
                }
            }

        }
    }
}
