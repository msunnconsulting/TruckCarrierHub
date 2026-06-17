--TransportCompany_DriverType

	CREATE TABLE [dbo].[TransportCompany_DriverType] (
    [USDOTNumber] INT NOT NULL,
    [DriverNumber] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([USDOTNumber] ASC, [DriverNumber] ASC),
    CONSTRAINT [FK_Main_DriverType_DriverType] FOREIGN KEY ([DriverNumber]) REFERENCES [dbo].[DriverType] ([DriverNumber]) ON DELETE CASCADE,
    CONSTRAINT [FK_Main_TransportCompany_DriverType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[TransportCompany] ([USDOTNumber]) ON DELETE CASCADE
	);


