--Admin
	
	ALTER TABLE Admin ADD GlobalHiring int;
	ALTER TABLE Admin ADD SuccessStoryPublished bit;
	ALTER TABLE Admin ADD SuccessStories text;
	GO
	Update Admin SET SuccessStoryPublished = 'False'
	
--Business

	ALTER TABLE Business ADD PasswordHash VARCHAR(256);
	ALTER TABLE Business ADD PasswordSalt VARCHAR(64);
	ALTER TABLE Business ADD BusinessContactEmail NVARCHAR(255);
	ALTER TABLE Business ADD JobContactEmail Nvarchar(255);
	ALTER TABLE Business ADD JobContactPhone Nvarchar(255);
	ALTER TABLE Business ADD JobContactSMS Nvarchar(255);
	ALTER TABLE Business ADD NowHiring Bit NOT NULL DEFAULT 0;
	ALTER TABLE Business ADD ForgotPasswordKey VARCHAR(50);
	ALTER TABLE Business DROP COLUMN EmailPublished;