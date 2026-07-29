CREATE TABLE [dbo].[McmisCountyCodes] (
    [Id]         INT           NOT NULL,
    [StateCode]  CHAR (2)      NOT NULL,
    [CountyCode] INT           NOT NULL,
    [CountyName] VARCHAR (100) NOT NULL,
    CONSTRAINT [PK__McmisCou__3214EC07FAF80B03] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_McmisCountyCodes]
    ON [dbo].[McmisCountyCodes]([StateCode] ASC, [CountyCode] ASC);
