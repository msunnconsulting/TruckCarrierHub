CREATE TABLE [dbo].[TransportCompany_CargoType] (
    [USDOTNumber] INT NOT NULL,
    [CargoNumber] INT NOT NULL,
    CONSTRAINT [FK_Main_CargoType_CargoType] FOREIGN KEY ([CargoNumber]) REFERENCES [dbo].[CargoType] ([CargoNumber]) ON DELETE CASCADE,
    CONSTRAINT [FK_Main_CargoType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[TransportCompany] ([USDOTNumber]) ON DELETE CASCADE
);

