--DriverType

	CREATE TABLE [dbo].[DriverType] (
    [DriverNumber]     INT            IDENTITY (1, 1) NOT NULL,
    [DriverName]       NVARCHAR (MAX) NOT NULL,
    [DriverNameForUrl] NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Driver] PRIMARY KEY CLUSTERED ([DriverNumber] ASC)
	);

