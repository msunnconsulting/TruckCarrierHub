CREATE TABLE [dbo].[Business] (
    [Id]                    INT              IDENTITY (1, 1) NOT NULL,
    [USDOTNumber]           INT              NOT NULL,
    [Website]               NVARCHAR (255)   NOT NULL,
    [EmailVerified]         BIT              NULL,
    [WebsiteApproved]       BIT              NULL,
    [EmailPublished]        BIT              NULL,
    [CommunicationApproved] BIT              NULL,
    [CreatedDate]           DATETIME         NULL,
    [UpdatedDate]           DATETIME         NULL,
    [VerificationKey]       UNIQUEIDENTIFIER NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Business_TransportCompany] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[TransportCompany] ([USDOTNumber])
);