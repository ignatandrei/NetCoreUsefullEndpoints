using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Playwright;
using Microsoft.Playwright.MSTest;
using System.Text.Json;
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
        await base.Playwright.Chromium.LaunchAsync(new()
        {
            Headless = false,
        });
        await Page.GotoAsync(baseUrl+"swagger/index.html");
        var data= await Page.ScreenshotAsync();
        await System.IO.File.WriteAllBytesAsync("swagger.png", data);
        using var openApiDocument = JsonDocument.Parse(content);
        if (!Directory.Exists(pathVideos))
            Directory.CreateDirectory(pathVideos);
        foreach (var path in openApiDocument.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var op in path.Value.EnumerateObject())
            {
                if (op.Name ==  "get")
                {
                    if (!op.Value.TryGetProperty("parameters", out var parameters) || parameters.GetArrayLength() == 0)
                    {
                        var url = baseUrl + path.Name;
                        url = url.Replace("//", "/");
                        //await Page.GotoAsync(url);
                        var name = path.Name.Replace("/", "_");
                        string pathVideo = pathVideos + name;
                        var cntVideo = await base.NewContextAsync(new()
                        {
                            RecordVideoDir=pathVideo,
                            
                        });
                        var page = await cntVideo.NewPageAsync();
                        await page.GotoAsync(baseUrl + "swagger/index.html");
                        await page.WaitForLoadStateAsync();
                        await page.ScreenshotAsync();
                        var element = page.GetByText(path.Name, new PageGetByTextOptions()
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
                        Directory.Delete(pathVideo);
                        //return;
                    }
                }
            }

        }
    }
}
