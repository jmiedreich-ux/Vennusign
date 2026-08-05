using Vennu.Core.Models;
using Vennu.Data.Services;

namespace Vennu.DataAccess.Tests;

public sealed class CapabilityDecisionEngineTests
{
    private static readonly CapabilityId Capability = CapabilityId.Parse("publishing.release.publish");
    private readonly CapabilityDecisionEngine engine = new();

    [Fact]
    public void AllSatisfied_ReturnsAllowedContract()
    {
        var result = engine.Evaluate(Input(AllSatisfied()));

        Assert.Equal(CapabilityDecisionOutcome.Allowed, result.Decision);
        Assert.Equal("decision.allowed", result.ReasonCode);
        Assert.Equal(CapabilityDecisionCategory.None, result.Category);
        Assert.Equal(Capability, result.Capability);
        Assert.Equal("correlation-1", result.CorrelationId);
        Assert.Empty(result.Conditions);
    }

    [Fact]
    public void Condition_ReturnsAllowedWithConditionsWithoutHidingDimension()
    {
        var dimensions = AllSatisfied();
        dimensions[CapabilityDecisionCategory.ResourceState] = CapabilityDecisionDimension.Condition(
            CapabilityDecisionCategory.ResourceState,
            "delivery.offline_queued",
            "decisions.delivery.offline_queued",
            new Dictionary<string, string> { ["screen"] = "Patio" },
            "publish_and_confirm_after_reconnect");

        var result = engine.Evaluate(Input(dimensions));

        Assert.Equal(CapabilityDecisionOutcome.AllowedWithConditions, result.Decision);
        var condition = Assert.Single(result.Conditions);
        Assert.Equal(CapabilityDecisionCategory.ResourceState, condition.Category);
        Assert.Equal("delivery.offline_queued", condition.ReasonCode);
        Assert.Equal("Patio", condition.Parameters["screen"]);
    }

    [Theory]
    [InlineData(CapabilityDecisionCategory.IdentityContext, CapabilityDecisionOutcome.Denied, "identity.required")]
    [InlineData(CapabilityDecisionCategory.Rollout, CapabilityDecisionOutcome.TemporarilyBlocked, "rollout.temporary")]
    [InlineData(CapabilityDecisionCategory.Entitlement, CapabilityDecisionOutcome.Denied, "entitlement.required")]
    [InlineData(CapabilityDecisionCategory.Permission, CapabilityDecisionOutcome.Denied, "permission.required")]
    [InlineData(CapabilityDecisionCategory.AddOn, CapabilityDecisionOutcome.Unavailable, "add_on.required")]
    [InlineData(CapabilityDecisionCategory.Allowance, CapabilityDecisionOutcome.Denied, "allowance.reached")]
    [InlineData(CapabilityDecisionCategory.ResourceState, CapabilityDecisionOutcome.TemporarilyBlocked, "resource_state.blocked")]
    [InlineData(CapabilityDecisionCategory.RequestValidity, CapabilityDecisionOutcome.Denied, "request.invalid")]
    public void FailedDimension_ReturnsItsTruthfulCategoryAndReason(
        CapabilityDecisionCategory category,
        CapabilityDecisionOutcome outcome,
        string reasonCode)
    {
        var dimensions = AllSatisfied();
        dimensions[category] = CapabilityDecisionDimension.Failed(
            category,
            outcome,
            reasonCode,
            $"decisions.{reasonCode}",
            resolution: "resolve_and_retry",
            retryAfter: outcome == CapabilityDecisionOutcome.TemporarilyBlocked ? TimeSpan.FromMinutes(5) : null);

        var result = engine.Evaluate(Input(dimensions));

        Assert.Equal(outcome, result.Decision);
        Assert.Equal(category, result.Category);
        Assert.Equal(reasonCode, result.ReasonCode);
        Assert.Equal("resolve_and_retry", result.Resolution);
    }

    [Fact]
    public void Priority_IsDeterministicAndDoesNotReplacePermissionWithUpgradeReason()
    {
        var dimensions = AllSatisfied();
        dimensions[CapabilityDecisionCategory.Entitlement] = CapabilityDecisionDimension.Failed(
            CapabilityDecisionCategory.Entitlement,
            CapabilityDecisionOutcome.Denied,
            "entitlement.required",
            "decisions.entitlement.required");
        dimensions[CapabilityDecisionCategory.Permission] = CapabilityDecisionDimension.Failed(
            CapabilityDecisionCategory.Permission,
            CapabilityDecisionOutcome.Denied,
            "permission.required",
            "decisions.permission.required");

        var result = engine.Evaluate(Input(dimensions));

        Assert.Equal(CapabilityDecisionCategory.Entitlement, result.Category);
        Assert.Equal("entitlement.required", result.ReasonCode);
    }

    [Fact]
    public void MissingDimension_FailsClosedWithDiagnosticParameter()
    {
        var dimensions = AllSatisfied();
        dimensions.Remove(CapabilityDecisionCategory.Permission);

        var result = engine.Evaluate(Input(dimensions));

        Assert.Equal(CapabilityDecisionOutcome.Unavailable, result.Decision);
        Assert.Equal("decision.input_incomplete", result.ReasonCode);
        Assert.Contains("Permission", result.Parameters["dimensions"]);
    }

    [Fact]
    public void BatchEvaluation_PreservesInputOrderAndCorrelation()
    {
        var second = Input(AllSatisfied()) with
        {
            Capability = CapabilityId.Parse("screen.device.pair"),
            CorrelationId = "correlation-2"
        };

        var results = engine.EvaluateBatch([Input(AllSatisfied()), second]).ToArray();

        Assert.Equal([Capability, second.Capability], results.Select(item => item.Capability));
        Assert.Equal(["correlation-1", "correlation-2"], results.Select(item => item.CorrelationId));
    }

    private static CapabilityDecisionInput Input(Dictionary<CapabilityDecisionCategory, CapabilityDecisionDimension> dimensions) =>
        new(Capability, dimensions.Values.ToArray(), "correlation-1", "en-US");

    private static Dictionary<CapabilityDecisionCategory, CapabilityDecisionDimension> AllSatisfied() =>
        Enum.GetValues<CapabilityDecisionCategory>()
            .Where(category => category is not CapabilityDecisionCategory.None and not CapabilityDecisionCategory.Capability)
            .ToDictionary(category => category, CapabilityDecisionDimension.Satisfied);
}
