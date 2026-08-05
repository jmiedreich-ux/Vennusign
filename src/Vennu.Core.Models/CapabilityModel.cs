using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Vennu.Core.Models;

public readonly partial record struct CapabilityId
{
    private const int MaximumLength = 120;

    private CapabilityId(string value) => Value = value;

    public string Value { get; }

    public static CapabilityId Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || value.Length > MaximumLength
            || value != value.Trim()
            || !CapabilityPattern().IsMatch(value))
        {
            throw new FormatException("Capability IDs must use lowercase domain.resource.action segments.");
        }

        return new CapabilityId(value);
    }

    public static bool TryParse(string? value, out CapabilityId capabilityId)
    {
        try
        {
            capabilityId = Parse(value);
            return true;
        }
        catch (FormatException)
        {
            capabilityId = default;
            return false;
        }
    }

    public override string ToString() => Value ?? string.Empty;

    [GeneratedRegex("^[a-z][a-z0-9]*(?:_[a-z0-9]+)*\\.[a-z][a-z0-9]*(?:_[a-z0-9]+)*\\.[a-z][a-z0-9]*(?:_[a-z0-9]+)*$")]
    private static partial Regex CapabilityPattern();
}

public enum CapabilityDomain
{
    Content = 1,
    Publishing = 2,
    Screen = 3,
    Schedule = 4,
    Workflow = 5,
    Organization = 6,
    Localization = 7,
    Analytics = 8,
    Branding = 9,
    Account = 10,
    Support = 11
}

public enum CapabilityClassification
{
    UniversalCore = 1,
    AdvancedNative = 2,
    Governance = 3,
    Deferred = 4
}

public enum CapabilityOperationKind
{
    Read = 1,
    Change = 2,
    Administration = 3
}

public sealed record CapabilityDefinition(
    CapabilityId Id,
    CapabilityDomain Domain,
    CapabilityClassification Classification,
    CapabilityOperationKind OperationKind,
    string NameMessageKey,
    string DescriptionMessageKey);

public static class Version1CapabilityRegistry
{
    private static readonly ReadOnlyCollection<CapabilityDefinition> DefinitionsValue = Array.AsReadOnly(
    [
        Core("content.item.create", CapabilityDomain.Content, CapabilityOperationKind.Change),
        Core("content.item.update", CapabilityDomain.Content, CapabilityOperationKind.Change),
        Core("content.item.archive", CapabilityDomain.Content, CapabilityOperationKind.Change),
        Core("content.item.availability_update", CapabilityDomain.Content, CapabilityOperationKind.Change),
        Core("content.item.dietary_information_manage", CapabilityDomain.Content, CapabilityOperationKind.Change),
        Advanced("content.collection.bulk_update", CapabilityDomain.Content, CapabilityOperationKind.Change),
        Advanced("content.source.synchronize", CapabilityDomain.Content, CapabilityOperationKind.Change),

        Core("publishing.release.preview", CapabilityDomain.Publishing, CapabilityOperationKind.Read),
        Core("publishing.release.publish", CapabilityDomain.Publishing, CapabilityOperationKind.Change),
        Core("publishing.release.confirm", CapabilityDomain.Publishing, CapabilityOperationKind.Read),
        Core("publishing.release.replace", CapabilityDomain.Publishing, CapabilityOperationKind.Change),
        Core("publishing.release.unpublish", CapabilityDomain.Publishing, CapabilityOperationKind.Change),
        Core("publishing.delivery.retry", CapabilityDomain.Publishing, CapabilityOperationKind.Change),
        Core("publishing.delivery.restore", CapabilityDomain.Publishing, CapabilityOperationKind.Change),

        Core("screen.device.view", CapabilityDomain.Screen, CapabilityOperationKind.Read),
        Core("screen.device.pair", CapabilityDomain.Screen, CapabilityOperationKind.Change),
        Core("screen.device.unpair", CapabilityDomain.Screen, CapabilityOperationKind.Change),
        Core("screen.content.target", CapabilityDomain.Screen, CapabilityOperationKind.Change),
        Core("screen.delivery.view", CapabilityDomain.Screen, CapabilityOperationKind.Read),
        Core("screen.delivery.recover", CapabilityDomain.Screen, CapabilityOperationKind.Change),
        Advanced("screen.wall.coordinate", CapabilityDomain.Screen, CapabilityOperationKind.Change),

        Core("schedule.entry.manage", CapabilityDomain.Schedule, CapabilityOperationKind.Change),
        Advanced("schedule.rotation.manage", CapabilityDomain.Schedule, CapabilityOperationKind.Change),
        Advanced("schedule.promotion.automate", CapabilityDomain.Schedule, CapabilityOperationKind.Change),
        Advanced("schedule.conflict.resolve", CapabilityDomain.Schedule, CapabilityOperationKind.Change),

        Advanced("workflow.approval.request", CapabilityDomain.Workflow, CapabilityOperationKind.Change),
        Advanced("workflow.approval.review", CapabilityDomain.Workflow, CapabilityOperationKind.Change),
        Advanced("workflow.assignment.manage", CapabilityDomain.Workflow, CapabilityOperationKind.Administration),

        Core("organization.venue.create", CapabilityDomain.Organization, CapabilityOperationKind.Change),
        Governance("organization.venue.manage", CapabilityDomain.Organization, CapabilityOperationKind.Administration),
        Governance("organization.content.bulk_publish", CapabilityDomain.Organization, CapabilityOperationKind.Change),
        Governance("organization.template.manage", CapabilityDomain.Organization, CapabilityOperationKind.Administration),

        Core("localization.variant.manage", CapabilityDomain.Localization, CapabilityOperationKind.Change),
        Advanced("localization.variant.review", CapabilityDomain.Localization, CapabilityOperationKind.Change),
        Advanced("localization.translation.automate", CapabilityDomain.Localization, CapabilityOperationKind.Change),

        Core("analytics.delivery_health.view", CapabilityDomain.Analytics, CapabilityOperationKind.Read),
        Advanced("analytics.operations.view", CapabilityDomain.Analytics, CapabilityOperationKind.Read),
        Governance("analytics.portfolio.view", CapabilityDomain.Analytics, CapabilityOperationKind.Read),
        Governance("analytics.report.export", CapabilityDomain.Analytics, CapabilityOperationKind.Read),

        Core("branding.theme.manage", CapabilityDomain.Branding, CapabilityOperationKind.Change),
        Advanced("branding.layout.manage", CapabilityDomain.Branding, CapabilityOperationKind.Change),
        Governance("branding.standard.manage", CapabilityDomain.Branding, CapabilityOperationKind.Administration),
        Deferred("branding.custom_content.manage", CapabilityDomain.Branding, CapabilityOperationKind.Change),

        Core("account.profile.manage", CapabilityDomain.Account, CapabilityOperationKind.Change),
        Core("account.security.manage", CapabilityDomain.Account, CapabilityOperationKind.Change),
        Core("account.billing.view", CapabilityDomain.Account, CapabilityOperationKind.Read),
        Core("account.billing.manage", CapabilityDomain.Account, CapabilityOperationKind.Administration),
        Governance("account.member.manage", CapabilityDomain.Account, CapabilityOperationKind.Administration),

        Governance("support.context.enter", CapabilityDomain.Support, CapabilityOperationKind.Administration),
        Governance("support.entitlement.override", CapabilityDomain.Support, CapabilityOperationKind.Administration),
        Governance("support.allowance.override", CapabilityDomain.Support, CapabilityOperationKind.Administration)
    ]);

    private static readonly IReadOnlyDictionary<CapabilityId, CapabilityDefinition> ByIdValue =
        new ReadOnlyDictionary<CapabilityId, CapabilityDefinition>(DefinitionsValue.ToDictionary(item => item.Id));

    public static IReadOnlyList<CapabilityDefinition> Definitions => DefinitionsValue;

    public static IReadOnlyDictionary<CapabilityId, CapabilityDefinition> ById => ByIdValue;

    public static CapabilityDefinition Get(CapabilityId id) =>
        ByIdValue.TryGetValue(id, out var definition)
            ? definition
            : throw new KeyNotFoundException($"Capability '{id}' is not registered.");

    private static CapabilityDefinition Core(string id, CapabilityDomain domain, CapabilityOperationKind operationKind) =>
        Create(id, domain, CapabilityClassification.UniversalCore, operationKind);

    private static CapabilityDefinition Advanced(string id, CapabilityDomain domain, CapabilityOperationKind operationKind) =>
        Create(id, domain, CapabilityClassification.AdvancedNative, operationKind);

    private static CapabilityDefinition Governance(string id, CapabilityDomain domain, CapabilityOperationKind operationKind) =>
        Create(id, domain, CapabilityClassification.Governance, operationKind);

    private static CapabilityDefinition Deferred(string id, CapabilityDomain domain, CapabilityOperationKind operationKind) =>
        Create(id, domain, CapabilityClassification.Deferred, operationKind);

    private static CapabilityDefinition Create(
        string id,
        CapabilityDomain domain,
        CapabilityClassification classification,
        CapabilityOperationKind operationKind)
    {
        var capabilityId = CapabilityId.Parse(id);
        var messageStem = $"capabilities.{id}";
        return new CapabilityDefinition(
            capabilityId,
            domain,
            classification,
            operationKind,
            $"{messageStem}.name",
            $"{messageStem}.description");
    }
}
