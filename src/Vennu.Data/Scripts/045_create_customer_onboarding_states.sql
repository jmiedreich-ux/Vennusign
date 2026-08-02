CREATE TABLE dbo.CustomerOnboardingStates
(
    UserId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerOnboardingStates PRIMARY KEY,
    OrganizationId UNIQUEIDENTIFIER NULL,
    SelectedTierId UNIQUEIDENTIFIER NULL,
    VenueId UNIQUEIDENTIFIER NULL,
    FirstScreenId UNIQUEIDENTIFIER NULL,
    CreatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_CustomerOnboardingStates_CreatedUtc DEFAULT SYSUTCDATETIME(),
    UpdatedUtc DATETIME2(7) NOT NULL CONSTRAINT DF_CustomerOnboardingStates_UpdatedUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CustomerOnboardingStates_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT FK_CustomerOnboardingStates_Organization FOREIGN KEY (OrganizationId) REFERENCES dbo.Organizations (Id),
    CONSTRAINT FK_CustomerOnboardingStates_Tier FOREIGN KEY (SelectedTierId) REFERENCES dbo.SubscriptionTiers (Id),
    CONSTRAINT FK_CustomerOnboardingStates_Venue FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT FK_CustomerOnboardingStates_FirstScreen FOREIGN KEY (FirstScreenId) REFERENCES dbo.Screens (Id)
);

CREATE UNIQUE INDEX UX_CustomerOnboardingStates_OrganizationId
    ON dbo.CustomerOnboardingStates (OrganizationId)
    WHERE OrganizationId IS NOT NULL;

CREATE INDEX IX_CustomerOnboardingStates_UpdatedUtc
    ON dbo.CustomerOnboardingStates (UpdatedUtc DESC);
