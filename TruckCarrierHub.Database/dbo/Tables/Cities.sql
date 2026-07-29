CREATE TABLE [dbo].[Cities] (
    [CountryCode]        NVARCHAR (5)    NULL,
    [StateCode]          NVARCHAR (255)  NULL,
    [CityName]           NVARCHAR (4000) NULL,
    [NumberOfCompanies]  INT             NULL,
    [Article]            TEXT            NULL,
    [CityArticleAllowed] BIT             NULL,
    [Description]        NVARCHAR (MAX)  NULL,
    [LastRenewedDate]    DATETIME        NULL
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UQ_Cities_StateCity]
    ON [dbo].[Cities]([StateCode] ASC, [CityName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Cities_CityName]
    ON [dbo].[Cities]([CityName] ASC, [StateCode] ASC)
    INCLUDE ([NumberOfCompanies]);
