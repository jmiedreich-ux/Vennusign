ALTER TABLE dbo.CustomerAuthSessions
ADD Assurance INT NOT NULL CONSTRAINT DF_CustomerAuthSessions_Assurance DEFAULT (1),
    StepUpUtc DATETIME2 NULL;
GO

CREATE TABLE dbo.CustomerPasskeyCredentials (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerPasskeyCredentials PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CredentialId VARBINARY(1024) NOT NULL,
    PublicKey VARBINARY(MAX) NOT NULL,
    UserHandle VARBINARY(64) NOT NULL,
    SignatureCounter BIGINT NOT NULL CONSTRAINT DF_CustomerPasskeys_Counter DEFAULT (0),
    DisplayName NVARCHAR(100) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL,
    LastUsedUtc DATETIME2 NULL,
    RevokedUtc DATETIME2 NULL,
    CONSTRAINT FK_CustomerPasskeys_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id),
    CONSTRAINT UQ_CustomerPasskeys_Credential UNIQUE (CredentialId)
);
GO

CREATE TABLE dbo.CustomerTotpAuthenticators (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerTotpAuthenticators PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    ProtectedSecret NVARCHAR(2000) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL,
    VerifiedUtc DATETIME2 NULL,
    LastAcceptedCounter BIGINT NULL,
    RevokedUtc DATETIME2 NULL,
    CONSTRAINT FK_CustomerTotp_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id)
);
GO
CREATE UNIQUE INDEX UX_CustomerTotp_ActiveUser ON dbo.CustomerTotpAuthenticators(UserId) WHERE RevokedUtc IS NULL;
GO

CREATE TABLE dbo.CustomerRecoveryCodes (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerRecoveryCodes PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    CodeHash CHAR(64) NOT NULL,
    CreatedUtc DATETIME2 NOT NULL,
    UsedUtc DATETIME2 NULL,
    CONSTRAINT FK_CustomerRecoveryCodes_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id),
    CONSTRAINT UQ_CustomerRecoveryCodes_Hash UNIQUE (CodeHash)
);
GO

CREATE TABLE dbo.CustomerAuthenticationChallenges (
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerAuthenticationChallenges PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type INT NOT NULL,
    ProtectedOptions NVARCHAR(MAX) NOT NULL,
    ExpiresUtc DATETIME2 NOT NULL,
    ConsumedUtc DATETIME2 NULL,
    CreatedUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_CustomerAuthenticationChallenges_User FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers(Id),
    CONSTRAINT CK_CustomerAuthenticationChallenges_Type CHECK (Type IN (1, 2))
);
GO
CREATE INDEX IX_CustomerAuthenticationChallenges_Expiry ON dbo.CustomerAuthenticationChallenges(ExpiresUtc, ConsumedUtc);
GO
