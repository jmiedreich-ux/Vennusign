/*
    Menus 6-A1: resumable paste/import review sessions.

    WHAT THIS DISCARDS: nothing. These tables contain temporary review state and
    do not alter the menu, item-library, publish, assignment, or availability models.
*/

CREATE TABLE dbo.MenuImportSessions
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_MenuImportSessions PRIMARY KEY,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    RawPaste NVARCHAR(MAX) NOT NULL,
    ParseRevision BIGINT NOT NULL CONSTRAINT DF_MenuImportSessions_ParseRevision DEFAULT 1,
    Status NVARCHAR(20) NOT NULL CONSTRAINT DF_MenuImportSessions_Status DEFAULT N'reviewing',
    LineCount INT NOT NULL,
    ItemCount INT NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    UpdatedUtc DATETIME2(7) NOT NULL,
    UpdatedBy NVARCHAR(320) NULL,
    Revision ROWVERSION NOT NULL,
    CONSTRAINT FK_MenuImportSessions_Venues FOREIGN KEY (VenueId) REFERENCES dbo.Venues (Id),
    CONSTRAINT UQ_MenuImportSessions_Id_Venue UNIQUE (Id, VenueId),
    CONSTRAINT CK_MenuImportSessions_Status CHECK (Status IN (N'reviewing', N'resolved')),
    CONSTRAINT CK_MenuImportSessions_Counts CHECK (LineCount >= 0 AND ItemCount >= 0),
    CONSTRAINT CK_MenuImportSessions_Revision CHECK (ParseRevision > 0),
    CONSTRAINT CK_MenuImportSessions_Expiry CHECK (ExpiresUtc > CreatedUtc)
);
CREATE INDEX IX_MenuImportSessions_VenueExpiry ON dbo.MenuImportSessions (VenueId, ExpiresUtc) INCLUDE (Status, ParseRevision);
GO

CREATE TABLE dbo.MenuImportSourceLines
(
    SessionId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    LineNumber INT NOT NULL,
    RawText NVARCHAR(MAX) NOT NULL,
    Disposition NVARCHAR(24) NOT NULL,
    ParsedName NVARCHAR(200) NULL,
    ParsedDescription NVARCHAR(1000) NULL,
    ParsedPrice NVARCHAR(12) NULL,
    ParserReason NVARCHAR(80) NULL,
    ParseRevision BIGINT NOT NULL,
    CONSTRAINT PK_MenuImportSourceLines PRIMARY KEY (SessionId, LineNumber),
    CONSTRAINT FK_MenuImportSourceLines_Session FOREIGN KEY (SessionId, VenueId) REFERENCES dbo.MenuImportSessions (Id, VenueId),
    CONSTRAINT UQ_MenuImportSourceLines_KeyVenue UNIQUE (SessionId, LineNumber, VenueId),
    CONSTRAINT CK_MenuImportSourceLines_Line CHECK (LineNumber > 0),
    CONSTRAINT CK_MenuImportSourceLines_Disposition CHECK (Disposition IN (N'blank', N'section', N'item', N'unresolved', N'fallback')),
    CONSTRAINT CK_MenuImportSourceLines_Revision CHECK (ParseRevision > 0)
);
GO

CREATE TABLE dbo.MenuImportReviewQuestions
(
    SessionId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    QuestionKey NVARCHAR(80) NOT NULL,
    Fingerprint CHAR(64) NOT NULL,
    Kind NVARCHAR(32) NOT NULL,
    DisplayOrder INT NOT NULL,
    Required BIT NOT NULL CONSTRAINT DF_MenuImportReviewQuestions_Required DEFAULT 1,
    ParseRevision BIGINT NOT NULL,
    CONSTRAINT PK_MenuImportReviewQuestions PRIMARY KEY (SessionId, QuestionKey),
    CONSTRAINT FK_MenuImportReviewQuestions_Session FOREIGN KEY (SessionId, VenueId) REFERENCES dbo.MenuImportSessions (Id, VenueId),
    CONSTRAINT UQ_MenuImportReviewQuestions_KeyVenue UNIQUE (SessionId, QuestionKey, VenueId),
    CONSTRAINT CK_MenuImportReviewQuestions_Key CHECK (LEN(LTRIM(RTRIM(QuestionKey))) > 0),
    CONSTRAINT CK_MenuImportReviewQuestions_Fingerprint CHECK (LEN(Fingerprint) = 64),
    CONSTRAINT CK_MenuImportReviewQuestions_Kind CHECK (Kind IN (N'identity', N'unreadable')),
    CONSTRAINT CK_MenuImportReviewQuestions_Order CHECK (DisplayOrder >= 0),
    CONSTRAINT CK_MenuImportReviewQuestions_Revision CHECK (ParseRevision > 0)
);
GO

CREATE TABLE dbo.MenuImportQuestionLines
(
    SessionId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    QuestionKey NVARCHAR(80) NOT NULL,
    LineNumber INT NOT NULL,
    CONSTRAINT PK_MenuImportQuestionLines PRIMARY KEY (SessionId, QuestionKey, LineNumber),
    CONSTRAINT FK_MenuImportQuestionLines_Question FOREIGN KEY (SessionId, QuestionKey, VenueId) REFERENCES dbo.MenuImportReviewQuestions (SessionId, QuestionKey, VenueId),
    CONSTRAINT FK_MenuImportQuestionLines_Line FOREIGN KEY (SessionId, LineNumber, VenueId) REFERENCES dbo.MenuImportSourceLines (SessionId, LineNumber, VenueId)
);
GO

CREATE TABLE dbo.MenuImportCandidates
(
    SessionId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    QuestionKey NVARCHAR(80) NOT NULL,
    ItemId UNIQUEIDENTIFIER NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    DisplayPrice NVARCHAR(12) NULL,
    MatchRule NVARCHAR(32) NOT NULL,
    IsSafe BIT NOT NULL,
    CONSTRAINT PK_MenuImportCandidates PRIMARY KEY (SessionId, QuestionKey, ItemId),
    CONSTRAINT FK_MenuImportCandidates_Question FOREIGN KEY (SessionId, QuestionKey, VenueId) REFERENCES dbo.MenuImportReviewQuestions (SessionId, QuestionKey, VenueId),
    CONSTRAINT FK_MenuImportCandidates_Item FOREIGN KEY (ItemId, VenueId) REFERENCES dbo.Items (Id, VenueId),
    CONSTRAINT CK_MenuImportCandidates_Rule CHECK (MatchRule IN (N'exact_normalized', N'semantic')),
    CONSTRAINT CK_MenuImportCandidates_Safety CHECK (IsSafe = 0 OR MatchRule = N'exact_normalized')
);
GO

CREATE TABLE dbo.MenuImportAnswers
(
    SessionId UNIQUEIDENTIFIER NOT NULL,
    VenueId UNIQUEIDENTIFIER NOT NULL,
    QuestionKey NVARCHAR(80) NOT NULL,
    Fingerprint CHAR(64) NOT NULL,
    Choice NVARCHAR(24) NOT NULL,
    SelectedItemId UNIQUEIDENTIFIER NULL,
    ParseRevision BIGINT NOT NULL,
    AnsweredUtc DATETIME2(7) NOT NULL,
    AnsweredBy NVARCHAR(320) NULL,
    CONSTRAINT PK_MenuImportAnswers PRIMARY KEY (SessionId, QuestionKey),
    CONSTRAINT FK_MenuImportAnswers_Question FOREIGN KEY (SessionId, QuestionKey, VenueId) REFERENCES dbo.MenuImportReviewQuestions (SessionId, QuestionKey, VenueId),
    CONSTRAINT FK_MenuImportAnswers_Candidate FOREIGN KEY (SessionId, QuestionKey, SelectedItemId) REFERENCES dbo.MenuImportCandidates (SessionId, QuestionKey, ItemId),
    CONSTRAINT CK_MenuImportAnswers_Choice CHECK (Choice IN (N'same_item', N'new_item', N'section', N'fallback')),
    CONSTRAINT CK_MenuImportAnswers_Shape CHECK ((Choice = N'same_item' AND SelectedItemId IS NOT NULL) OR (Choice <> N'same_item' AND SelectedItemId IS NULL)),
    CONSTRAINT CK_MenuImportAnswers_Fingerprint CHECK (LEN(Fingerprint) = 64),
    CONSTRAINT CK_MenuImportAnswers_Revision CHECK (ParseRevision > 0)
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.CapabilityDefinitions WHERE CapabilityId = 'content.menu.import.session_retention_minutes')
    INSERT dbo.CapabilityDefinitions (CapabilityId, Domain, Classification, OperationKind)
    VALUES ('content.menu.import.session_retention_minutes', 1, 1, 2);
GO

INSERT dbo.CapabilityAllowances (Id, OrganizationId, VenueId, CapabilityId, LimitValue, StartsUtc, EndsUtc)
SELECT NEWID(), venue.OrganizationId, venue.Id, 'content.menu.import.session_retention_minutes', 1440, SYSUTCDATETIME(), NULL
FROM dbo.Venues venue
WHERE venue.OrganizationId IS NOT NULL
  AND NOT EXISTS (
      SELECT 1 FROM dbo.CapabilityAllowances existing
      WHERE existing.VenueId = venue.Id
        AND existing.CapabilityId = 'content.menu.import.session_retention_minutes');
GO
