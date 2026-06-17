--Emails

	CREATE TABLE [dbo].[Emails] (
		[EmailID]     INT            IDENTITY (1, 1) NOT NULL,
		[Subject]       NVARCHAR (255) NOT NULL,
		[Content]       NVARCHAR (MAX) NOT NULL,
		[LinkNeeded]      bit NOT NULL,
		[CreatedDate]      datetime NOT NULL,
		[UpdatedDate]      datetime NOT NULL,		
		CONSTRAINT [PK_Email] PRIMARY KEY CLUSTERED ([EmailID] ASC)
	);
	


