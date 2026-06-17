CREATE TABLE [dbo].[CargoType] (
    [CargoNumber] INT            IDENTITY (1, 1) NOT NULL,
    [CargoName]   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_CargoType] PRIMARY KEY CLUSTERED ([CargoNumber] ASC)
);

