CREATE TABLE [dbo].[TransportCompany_ServiceType] (
    [USDOTNumber]       INT NOT NULL,
    [ServiceTypeNumber] INT NOT NULL,
    CONSTRAINT [FK_Main_ServiceType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[TransportCompany] ([USDOTNumber]) ON DELETE CASCADE,
    CONSTRAINT [FK_Main_ServiceType_ServiceType] FOREIGN KEY ([ServiceTypeNumber]) REFERENCES [dbo].[ServiceType] ([ServiceTypeNumber]) ON DELETE CASCADE
);

