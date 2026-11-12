using Aspire.Hosting;

namespace ExampleTests;

[TestClass]
public sealed class ApiExampleTests
{
    public TestContext TestContext { get; set; }

    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    private static DistributedApplication app;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        IDistributedApplicationTestingBuilder appHost = await DistributedApplicationTestingBuilder
             .CreateAsync<Projects.AspireExample_AppHost>();

        app = await appHost.BuildAsync(context.CancellationToken);
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await app.DisposeAsync();
    }

    [TestMethod]
    public async Task GetSlots_Call_Success()
    {
        HttpClient httpCLient = app.CreateHttpClient("exampleapi");

        string response = await httpCLient.GetStringAsync("/slots", this.TestContext.CancellationToken);
        Assert.AreNotEqual("", response);

        this.TestContext.WriteLine(response);
    }
}
