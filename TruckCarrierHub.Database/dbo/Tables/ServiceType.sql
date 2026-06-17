CREATE TABLE [dbo].[ServiceType] (
    [ServiceTypeNumber] INT            IDENTITY (1, 1) NOT NULL,
    [Service Type]      NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ServiceType] PRIMARY KEY CLUSTERED ([ServiceTypeNumber] ASC)
);

