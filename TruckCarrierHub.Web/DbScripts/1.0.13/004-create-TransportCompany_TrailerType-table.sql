--TransportCompany_TrailerType

	CREATE TABLE [dbo].[TransportCompany_TrailerType] (
    [USDOTNumber] INT NOT NULL,
    [TrailerNumber] INT NOT NULL,
    PRIMARY KEY CLUSTERED ([USDOTNumber] ASC, [TrailerNumber] ASC),
    CONSTRAINT [FK_Main_TrailerType_TrailerType] FOREIGN KEY ([TrailerNumber]) REFERENCES [dbo].[TrailerType] ([TrailerNumber]) ON DELETE CASCADE,
    CONSTRAINT [FK_Main_TransportCompany_TrailerType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[TransportCompany] ([USDOTNumber]) ON DELETE CASCADE
	);

