IF NOT EXISTS (SELECT 1 FROM dbo.Features WHERE [Key] = 'video_wall')
BEGIN
    INSERT dbo.Features (Id, [Key], Label, Category, IsActive)
    VALUES ('20000000-0000-0000-0000-000000000018', 'video_wall', 'Video Wall', 'layouts', 1);
END;
GO

INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue)
SELECT tier.Id, feature.Id, NULL
FROM dbo.SubscriptionTiers tier
CROSS JOIN dbo.Features feature
WHERE tier.Slug IN ('pro', 'business')
  AND feature.[Key] = 'video_wall'
  AND NOT EXISTS
  (
      SELECT 1
      FROM dbo.TierFeatures existing
      WHERE existing.TierId = tier.Id
        AND existing.FeatureId = feature.Id
  );
GO
