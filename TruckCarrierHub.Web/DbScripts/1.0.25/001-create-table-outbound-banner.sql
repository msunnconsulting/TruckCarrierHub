CREATE TABLE [dbo].[OutboundBanner] (
    [Id]               BIGINT         IDENTITY (1, 1) NOT NULL,
    [PageLevel]        TINYINT        NOT NULL,
    [IsShow]           BIT            DEFAULT ((0)) NOT NULL,
    [OriginalFileName] NVARCHAR (256) NULL,
    [FileName]         NVARCHAR (256) NULL,
    [URL]              NVARCHAR (MAX) NULL,
    [IsFollow]         BIT            DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);