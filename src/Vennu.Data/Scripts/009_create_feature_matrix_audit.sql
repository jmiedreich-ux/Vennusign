CREATE TABLE dbo.FeatureMatrixAudit
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_FeatureMatrixAudit PRIMARY KEY,
    TierId UNIQUEIDENTIFIER NOT NULL,
    FeatureId UNIQUEIDENTIFIER NOT NULL,
    AdminId NVARCHAR(150) NOT NULL,
    PreviousEnabled BIT NOT NULL,
    NewEnabled BIT NOT NULL,
    ChangedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_FeatureMatrixAudit_SubscriptionTiers
        FOREIGN KEY (TierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT FK_FeatureMatrixAudit_Features
        FOREIGN KEY (FeatureId) REFERENCES dbo.Features (Id),
    CONSTRAINT CK_FeatureMatrixAudit_ValueChanged
        CHECK (PreviousEnabled <> NewEnabled)
);
GO

CREATE INDEX IX_FeatureMatrixAudit_ChangedUtc
    ON dbo.FeatureMatrixAudit (ChangedUtc DESC)
    INCLUDE (TierId, FeatureId, AdminId, PreviousEnabled, NewEnabled);
GO

CREATE OR ALTER PROCEDURE dbo.usp_ApplyFeatureMatrixChanges
    @ChangesJson NVARCHAR(MAX),
    @AdminId NVARCHAR(150),
    @ChangedUtc DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF ISJSON(@ChangesJson) <> 1
        THROW 50001, 'Feature matrix changes must be valid JSON.', 1;

    IF NULLIF(LTRIM(RTRIM(@AdminId)), '') IS NULL
        THROW 50002, 'Admin identifier is required.', 1;

    BEGIN TRY
        BEGIN TRANSACTION;

        CREATE TABLE #RequestedChanges
        (
            TierId UNIQUEIDENTIFIER NOT NULL,
            FeatureId UNIQUEIDENTIFIER NOT NULL,
            Enabled BIT NOT NULL,
            PRIMARY KEY (TierId, FeatureId)
        );

        INSERT #RequestedChanges (TierId, FeatureId, Enabled)
        SELECT TierId, FeatureId, Enabled
        FROM OPENJSON(@ChangesJson)
        WITH
        (
            TierId UNIQUEIDENTIFIER '$.tierId',
            FeatureId UNIQUEIDENTIFIER '$.featureId',
            Enabled BIT '$.enabled'
        );

        IF EXISTS
        (
            SELECT 1
            FROM #RequestedChanges AS requested
            LEFT JOIN dbo.SubscriptionTiers AS tier ON tier.Id = requested.TierId
            LEFT JOIN dbo.Features AS feature ON feature.Id = requested.FeatureId AND feature.IsActive = 1
            WHERE tier.Id IS NULL OR feature.Id IS NULL
        )
            THROW 50003, 'Feature matrix changes contain an unknown tier or inactive feature.', 1;

        CREATE TABLE #EffectiveChanges
        (
            TierId UNIQUEIDENTIFIER NOT NULL,
            FeatureId UNIQUEIDENTIFIER NOT NULL,
            PreviousEnabled BIT NOT NULL,
            NewEnabled BIT NOT NULL,
            PRIMARY KEY (TierId, FeatureId)
        );

        INSERT #EffectiveChanges (TierId, FeatureId, PreviousEnabled, NewEnabled)
        SELECT
            requested.TierId,
            requested.FeatureId,
            CONVERT(BIT, CASE WHEN currentValue.TierId IS NULL THEN 0 ELSE 1 END),
            requested.Enabled
        FROM #RequestedChanges AS requested
        LEFT JOIN dbo.TierFeatures AS currentValue WITH (UPDLOCK, HOLDLOCK)
            ON currentValue.TierId = requested.TierId
           AND currentValue.FeatureId = requested.FeatureId
        WHERE (CASE WHEN currentValue.TierId IS NULL THEN 0 ELSE 1 END) <> requested.Enabled;

        INSERT dbo.FeatureMatrixAudit
            (Id, TierId, FeatureId, AdminId, PreviousEnabled, NewEnabled, ChangedUtc)
        SELECT NEWID(), TierId, FeatureId, @AdminId, PreviousEnabled, NewEnabled, @ChangedUtc
        FROM #EffectiveChanges;

        DELETE tierFeature
        FROM dbo.TierFeatures AS tierFeature
        INNER JOIN #EffectiveChanges AS change
            ON change.TierId = tierFeature.TierId
           AND change.FeatureId = tierFeature.FeatureId
        WHERE change.NewEnabled = 0;

        INSERT dbo.TierFeatures (TierId, FeatureId, LimitValue, CreatedUtc)
        SELECT TierId, FeatureId, NULL, @ChangedUtc
        FROM #EffectiveChanges
        WHERE NewEnabled = 1;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH;

    SELECT COUNT(*) AS ChangedCount
    FROM #EffectiveChanges;
END;
GO
