CREATE TABLE dbo.CustomerAuthSessions
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_CustomerAuthSessions PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash CHAR(64) NOT NULL,
    AuthenticationMethod INT NOT NULL,
    AuthenticatedUtc DATETIME2(7) NOT NULL,
    LastSeenUtc DATETIME2(7) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    RevokedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_CustomerAuthSessions_Users FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT UQ_CustomerAuthSessions_TokenHash UNIQUE (TokenHash),
    CONSTRAINT CK_CustomerAuthSessions_Method CHECK (AuthenticationMethod IN (1, 2, 3)),
    CONSTRAINT CK_CustomerAuthSessions_Lifetime CHECK
        (ExpiresUtc > CreatedUtc AND LastSeenUtc >= CreatedUtc AND LastSeenUtc <= ExpiresUtc),
    CONSTRAINT CK_CustomerAuthSessions_Revoke CHECK (RevokedUtc IS NULL OR RevokedUtc >= CreatedUtc)
);
GO

CREATE INDEX IX_CustomerAuthSessions_User_Active
    ON dbo.CustomerAuthSessions (UserId, ExpiresUtc DESC)
    WHERE RevokedUtc IS NULL;
GO

CREATE TABLE dbo.EmailLoginTokens
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_EmailLoginTokens PRIMARY KEY,
    UserId UNIQUEIDENTIFIER NOT NULL,
    TokenHash CHAR(64) NOT NULL,
    ReturnPath NVARCHAR(500) NOT NULL,
    ExpiresUtc DATETIME2(7) NOT NULL,
    ConsumedUtc DATETIME2(7) NULL,
    CreatedUtc DATETIME2(7) NOT NULL,
    CONSTRAINT FK_EmailLoginTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.CustomerUsers (Id),
    CONSTRAINT UQ_EmailLoginTokens_TokenHash UNIQUE (TokenHash),
    CONSTRAINT CK_EmailLoginTokens_ReturnPath CHECK
        (LEFT(ReturnPath, 1) = '/' AND LEFT(ReturnPath, 2) <> '//'),
    CONSTRAINT CK_EmailLoginTokens_Lifetime CHECK (ExpiresUtc > CreatedUtc),
    CONSTRAINT CK_EmailLoginTokens_Consumed CHECK
        (ConsumedUtc IS NULL OR (ConsumedUtc >= CreatedUtc AND ConsumedUtc <= ExpiresUtc))
);
GO

CREATE INDEX IX_EmailLoginTokens_User_Active
    ON dbo.EmailLoginTokens (UserId, ExpiresUtc DESC)
    WHERE ConsumedUtc IS NULL;
GO
