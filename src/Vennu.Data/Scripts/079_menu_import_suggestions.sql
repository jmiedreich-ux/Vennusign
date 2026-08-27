-- M6.11: what the rules could not place, suggested rather than asked.
--
-- After M6.8 and M6.9 a real four-page menu leaves two lines the deterministic parser cannot
-- classify: the restaurant's own name and its tagline, straddling a page break. No rule reaches
-- them - a heading and a restaurant name are the same shape - but a language model reads them
-- correctly and says why.
--
-- A18 governs what may be done with that. Nothing is pre-answered unless a rule can name why, and
-- a model cannot name a rule; so a suggestion is stored, shown, and applied only when the operator
-- says so. These columns hold the suggestion, never an answer.
--
-- The line-level verdict lives on the source line so the review screen can put the suggestion on
-- the row it is about, rather than in a banner detached from the thing it describes.
ALTER TABLE dbo.MenuImportSessions ADD
    SuggestedMenuName NVARCHAR(200) NULL,
    SuggestedMenuDescription NVARCHAR(500) NULL,
    ProposedMenuDescription NVARCHAR(500) NULL;
GO

ALTER TABLE dbo.MenuImportSourceLines ADD
    SuggestedVerdict NVARCHAR(24) NULL,
    SuggestedReason NVARCHAR(300) NULL;
GO

ALTER TABLE dbo.MenuImportSourceLines ADD CONSTRAINT CK_MenuImportSourceLines_SuggestedVerdict
    CHECK (SuggestedVerdict IS NULL OR SuggestedVerdict IN (N'menu_name', N'menu_description', N'section_heading', N'dish', N'leave_out'));
GO
