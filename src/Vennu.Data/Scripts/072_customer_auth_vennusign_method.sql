/*
    Adds CustomerAuthenticationMethod.Vennusign (7) for Entra External ID's own
    local account ("Sign in with Vennusign") - see
    docs/design/approved/authentication/decisions.md #2.

    WHAT THIS ALSO FIXES: CK_CustomerAuthSessions_Method only ever allowed
    (1, 2, 3) - Google, Apple, EmailLink - even though CustomerAuthenticationMethod
    has carried Passkey (4), Totp (5), and RecoveryCode (6) since they were added.
    A session issued with any of those three methods would have violated this
    constraint against a real database. Widened to the full current range (1-7)
    in the same migration that touches this constraint, rather than leaving that
    gap for the next person to rediscover.
*/

IF OBJECT_ID(N'dbo.CK_CustomerAuthSessions_Method', N'C') IS NOT NULL
    ALTER TABLE dbo.CustomerAuthSessions DROP CONSTRAINT CK_CustomerAuthSessions_Method;
ALTER TABLE dbo.CustomerAuthSessions ADD CONSTRAINT CK_CustomerAuthSessions_Method
    CHECK (AuthenticationMethod IN (1, 2, 3, 4, 5, 6, 7));
GO

IF OBJECT_ID(N'dbo.CK_ExternalIdentities_Provider', N'C') IS NOT NULL
    ALTER TABLE dbo.ExternalIdentities DROP CONSTRAINT CK_ExternalIdentities_Provider;
ALTER TABLE dbo.ExternalIdentities ADD CONSTRAINT CK_ExternalIdentities_Provider
    CHECK (Provider IN (1, 2, 3));
GO
