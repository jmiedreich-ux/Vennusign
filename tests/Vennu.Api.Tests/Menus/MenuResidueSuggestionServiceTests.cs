using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Vennu.Api.Menus;
using Vennu.Core.Models;

namespace Vennu.Api.Tests.Menus;

/// <summary>
/// The residue pass, tested where it can be tested.
///
/// What the model decides is not deterministic and is not asserted here. What the service does with
/// the answer is entirely deterministic, and every one of these is a way the import could be made
/// worse by a convenience: sending lines that were already settled, trusting a verdict about a line
/// nobody asked about, or letting a failed call take a working paste down with it.
/// </summary>
[Trait("Category", "Unit")]
public sealed class MenuResidueSuggestionServiceTests
{
    private static ParsedMenuPaste Parse(string paste) =>
        new MenuPasteParser().Parse(Guid.NewGuid(), Guid.NewGuid(), paste, 1, []);

    private const string MenuWithResidue = "Appetizers\nGarlic Bread $6.50\nMana-Thai Cuisine\nAll Natural Authentic Thai Cuisine\nSalads\nThai Salad $6.50";

    private static MenuResidueSuggestionService Service(HttpMessageHandler handler, MenuSuggestionOptions? options = null) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") },
            new Static(options ?? new MenuSuggestionOptions { ApiKey = "test-key" }),
            NullLogger<MenuResidueSuggestionService>.Instance);

    [Fact]
    public async Task WithNoKey_ItNeverCalls()
    {
        // Every environment without a key must behave exactly as it did before this shipped.
        var handler = new RecordingHandler("{}");
        var service = Service(handler, new MenuSuggestionOptions { ApiKey = null });

        Assert.False(service.Enabled);
        Assert.Null(await service.SuggestAsync(Parse(MenuWithResidue), default));
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task WithNothingUnplaced_ItNeverCalls()
    {
        // A menu the rules read whole costs nothing and waits for nothing.
        var handler = new RecordingHandler("{}");

        Assert.Null(await Service(handler).SuggestAsync(Parse("Appetizers\nGarlic Bread $6.50"), default));
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task ItSendsTheResidueAndItsNeighbours_NotTheMenu()
    {
        /*
         * The first version of this test asserted that no settled line is ever sent, and it failed -
         * correctly. Whether a line is a heading depends entirely on what follows it, so the two
         * lines either side go with it; a residue line sent alone cannot be judged at all.
         *
         * The claim in the service's own comment was wrong and was corrected, not the test. What is
         * actually guaranteed is narrower and is worth more: the payload is bounded to the residue
         * and its immediate neighbours, and - see the test below - a verdict about anything else is
         * discarded on the way back in.
         */
        var far = string.Join('\n', Enumerable.Range(1, 8).Select(number => $"Distant Dish {number} ${number}.00"));
        var handler = new RecordingHandler(Reply("""{"menuName":null,"menuDescription":null,"lines":[]}"""));
        await Service(handler).SuggestAsync(Parse($"Appetizers\n{far}\nMana-Thai Cuisine\nAll Natural Authentic Thai Cuisine"), default);

        Assert.Contains("Mana-Thai Cuisine", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("Distant Dish 8", handler.LastBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Distant Dish 1 ", handler.LastBody, StringComparison.Ordinal);
        Assert.DoesNotContain("Distant Dish 4", handler.LastBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AVerdictAboutALineNobodyAskedAboutIsDiscarded()
    {
        // The schema constrains the shape of the reply; it cannot constrain its line numbers. The
        // promise that only the residue is touched is enforced on the way back in, not merely asked
        // for on the way out.
        var handler = new RecordingHandler(Reply("""
            {"menuName":"Mana-Thai Cuisine","menuDescription":null,
             "lines":[{"lineNumber":2,"verdict":"leave_out","confidence":"high","why":"invented"},
                      {"lineNumber":3,"verdict":"menu_name","confidence":"high","why":"the restaurant"}]}
            """));

        var suggestion = await Service(handler).SuggestAsync(Parse(MenuWithResidue), default);

        Assert.NotNull(suggestion);
        Assert.Equal([3], suggestion!.Lines.Select(line => line.LineNumber));
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "{}")]
    [InlineData(HttpStatusCode.TooManyRequests, "{}")]
    [InlineData(HttpStatusCode.OK, "not json at all")]
    [InlineData(HttpStatusCode.OK, "{\"content\":[{\"type\":\"text\",\"text\":\"{ broken\"}]}")]
    public async Task AFailedCallLeavesTheImportExactlyAsItWas(HttpStatusCode status, string body)
    {
        // A convenience on a handful of lines may not be able to break a paste that otherwise works.
        Assert.Null(await Service(new RecordingHandler(body, status)).SuggestAsync(Parse(MenuWithResidue), default));
    }

    [Fact]
    public async Task ATimeoutIsNotAnError_AndIsNotAWait()
    {
        // An operator is watching this screen. The pass gives up and the review opens without it.
        var handler = new SlowHandler();
        var service = Service(handler, new MenuSuggestionOptions { ApiKey = "test-key", TimeoutSeconds = 1 });

        Assert.Null(await service.SuggestAsync(Parse(MenuWithResidue), default));
    }

    [Fact]
    public async Task ManyUnplacedLinesMeanTheParserIsWrong_NotTheMenuOdd()
    {
        // Thirteen lines the rules could not read is a parser defect, and asking a model to paper
        // over it is how the 91-question import would have been declared solved.
        var handler = new RecordingHandler("{}");
        var noise = string.Join('\n', Enumerable.Range(1, 13).Select(number => $"unreadable line {number} here"));

        Assert.Null(await Service(handler).SuggestAsync(Parse($"Appetizers\nGarlic Bread $6.50\n{noise}"), default));
        Assert.Equal(0, handler.Calls);
    }

    [Fact]
    public async Task ItAsksForOpus5AtLowEffort_WithAConstrainedReply()
    {
        var handler = new RecordingHandler(Reply("""{"menuName":null,"menuDescription":null,"lines":[]}"""));
        await Service(handler).SuggestAsync(Parse(MenuWithResidue), default);

        Assert.Contains("\"claude-opus-5\"", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("\"effort\":\"low\"", handler.LastBody, StringComparison.Ordinal);
        Assert.Contains("json_schema", handler.LastBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AMenuWhoseLineHoldsSeveralItemsStillGetsASuggestion()
    {
        /*
         * The regression. M6.9 let one pasted line hold several items, so a line number stopped
         * being unique - and this service still keyed a dictionary on it. It threw, the catch
         * swallowed it exactly as designed, and no menu containing a line like "Sides: ..." ever
         * received a suggestion. Nothing failed. Nothing was logged where anyone was looking.
         *
         * Found by probing the deployed environment with a real menu rather than by trusting a
         * suite whose fixtures had no such line in them.
         */
        var handler = new RecordingHandler(Reply("""{"menuName":"Mana-Thai Cuisine","menuDescription":null,"lines":[]}"""));
        // The Sides line must come last. Followed by a Title Case line the parser reads it as a
        // price set for the dishes below it, not as several items - correct behaviour that would
        // have made this fixture prove nothing. The guard below is why that was noticed.
        var paste = "Appetizers\nGarlic Bread $6.50\nMana-Thai Cuisine\nAll Natural Authentic Thai Cuisine\nSalads\nThai Salad $6.50\nSides: Jasmine Rice $2.00, Brown Rice $3.00, Peanut Sauce $2.00";
        var parsed = Parse(paste);

        Assert.True(parsed.Lines.GroupBy(line => line.LineNumber).Any(group => group.Count() > 1),
            "the fixture must contain a line holding several items, or it does not test the defect");

        var suggestion = await Service(handler).SuggestAsync(parsed, default);

        Assert.Equal("Mana-Thai Cuisine", suggestion?.MenuName);
        Assert.Equal(1, handler.Calls);
    }

    private static string Reply(string json) =>
        System.Text.Json.JsonSerializer.Serialize(new { content = new[] { new { type = "text", text = json } } });

    private sealed class Static(MenuSuggestionOptions value) : IOptions<MenuSuggestionOptions>
    {
        public MenuSuggestionOptions Value { get; } = value;
    }

    private sealed class RecordingHandler(string body, HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        public string LastBody { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Calls++;
            LastBody = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            return new HttpResponseMessage(status) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        }
    }

    private sealed class SlowHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
