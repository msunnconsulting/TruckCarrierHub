--add nullable field for enable log
ALTER TABLE AdminUser
    ADD IsDataMigrationLogEnabled BIT NULL;
	Go
	--update value
	UPDATE [AdminUser] SET [IsDataMigrationLogEnabled]=0 WHERE [IsDataMigrationLogEnabled] IS NULL;
	Go
	-- update datatype not null
	ALTER TABLE [AdminUser] ALTER COLUMN [IsDataMigrationLogEnabled] BIT NOT NULL;