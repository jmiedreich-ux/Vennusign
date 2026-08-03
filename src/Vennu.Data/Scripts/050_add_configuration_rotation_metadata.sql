ALTER TABLE dbo.SystemConfigurationDefinitions ADD RotationReminderDays INT NULL;
EXEC sys.sp_executesql N'ALTER TABLE dbo.SystemConfigurationDefinitions ADD CONSTRAINT CK_SystemConfigurationDefinitions_RotationReminderDays CHECK (RotationReminderDays IS NULL OR RotationReminderDays > 0);';
EXEC sys.sp_executesql N'UPDATE dbo.SystemConfigurationDefinitions SET RotationReminderDays = 90 WHERE IsSecret = 1;';
