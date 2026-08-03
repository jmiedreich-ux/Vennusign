ALTER TABLE dbo.Organizations ADD
    LegalName NVARCHAR(200) NULL,
    PrimaryContactName NVARCHAR(200) NULL,
    ContactEmail NVARCHAR(320) NULL,
    ContactPhone NVARCHAR(50) NULL,
    MailingAddress NVARCHAR(500) NULL;
GO

ALTER TABLE dbo.Organizations ADD CONSTRAINT CK_Organizations_Profile
CHECK (
    (LegalName IS NULL OR LEN(LTRIM(RTRIM(LegalName))) > 0) AND
    (PrimaryContactName IS NULL OR LEN(LTRIM(RTRIM(PrimaryContactName))) > 0) AND
    (ContactEmail IS NULL OR LEN(LTRIM(RTRIM(ContactEmail))) > 0) AND
    (ContactPhone IS NULL OR LEN(LTRIM(RTRIM(ContactPhone))) > 0) AND
    (MailingAddress IS NULL OR LEN(LTRIM(RTRIM(MailingAddress))) > 0)
);
GO
