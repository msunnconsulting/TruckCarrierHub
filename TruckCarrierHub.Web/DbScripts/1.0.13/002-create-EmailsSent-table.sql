--EmailsSent	 

	CREATE TABLE [dbo].[EmailsSent] (
		[USDOTNumber] INT NOT NULL,
		[EmailID] INT NOT NULL,
		[SentDate] DateTime NOT NULL,
		PRIMARY KEY CLUSTERED ([USDOTNumber] ASC, [EmailID] ASC),
		CONSTRAINT [FK_Main_Email_Email] FOREIGN KEY ([EmailID]) REFERENCES [dbo].[Emails] ([EmailID]) ON DELETE CASCADE,
		CONSTRAINT [FK_Main_TrailerType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[TransportCompany] ([USDOTNumber]) ON DELETE CASCADE
	);
	