--TrailerType

	CREATE TABLE [dbo].[TrailerType] (
    [TrailerNumber]     INT            IDENTITY (1, 1) NOT NULL,
    [TrailerName]       NVARCHAR (MAX) NOT NULL,
    [TrailerNameForUrl] NVARCHAR (50)  NULL,
    CONSTRAINT [PK_Trailer] PRIMARY KEY CLUSTERED ([TrailerNumber] ASC)
	);
	