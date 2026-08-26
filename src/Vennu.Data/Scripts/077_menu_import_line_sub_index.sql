-- Q216: one pasted line can hold several items.
--
-- "Sides: Jasmine Rice $2.00, Brown Rice $3.00, Peanut Sauce $2.00" is three items on one line, and
-- a real printed menu puts its sides, drinks and desserts that way. The source-line key allowed one
-- item per line, so fifteen items of a sixty-item menu could not be imported at all.
--
-- LineNumber keeps meaning what it says - a line of the pasted text - which the review screen's line
-- references and the never-drop-a-line invariant both rest on. LineSubIndex orders the items found
-- within one line. Existing rows take 0 and nothing is discarded.
--
-- MenuImportQuestionLines carries a foreign key into the key being replaced, so it is dropped and
-- rebuilt in the same transaction. A question is raised about a *line*, never about one item inside
-- a line - a line holding several priced items raises no question at all - so its own key gains a
-- sub-index of 0 and points at the first row of that line.
ALTER TABLE dbo.MenuImportQuestionLines DROP CONSTRAINT FK_MenuImportQuestionLines_Line;
GO

ALTER TABLE dbo.MenuImportSourceLines ADD LineSubIndex INT NOT NULL CONSTRAINT DF_MenuImportSourceLines_SubIndex DEFAULT 0;
GO

ALTER TABLE dbo.MenuImportSourceLines DROP CONSTRAINT UQ_MenuImportSourceLines_KeyVenue;
GO
ALTER TABLE dbo.MenuImportSourceLines DROP CONSTRAINT PK_MenuImportSourceLines;
GO

ALTER TABLE dbo.MenuImportSourceLines ADD CONSTRAINT PK_MenuImportSourceLines PRIMARY KEY (SessionId, LineNumber, LineSubIndex);
GO
ALTER TABLE dbo.MenuImportSourceLines ADD CONSTRAINT UQ_MenuImportSourceLines_KeyVenue UNIQUE (SessionId, LineNumber, LineSubIndex, VenueId);
GO

ALTER TABLE dbo.MenuImportQuestionLines ADD LineSubIndex INT NOT NULL CONSTRAINT DF_MenuImportQuestionLines_SubIndex DEFAULT 0;
GO
ALTER TABLE dbo.MenuImportQuestionLines ADD CONSTRAINT FK_MenuImportQuestionLines_Line
    FOREIGN KEY (SessionId, LineNumber, LineSubIndex, VenueId) REFERENCES dbo.MenuImportSourceLines (SessionId, LineNumber, LineSubIndex, VenueId);
GO
