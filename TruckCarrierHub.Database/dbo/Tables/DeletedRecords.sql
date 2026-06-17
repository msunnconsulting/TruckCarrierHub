CREATE TABLE [dbo].[DeletedRecords] (
    [USDOTNumber]     INT           NOT NULL,
    [RecordDeletedBy] INT           NOT NULL,
    [DeletedOn]       SMALLDATETIME CONSTRAINT [DF_DeletedRecords_DeletedOn] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_DeletedRecords] PRIMARY KEY CLUSTERED ([USDOTNumber] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 - By Owner, 
2 - By Admin', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DeletedRecords', @level2type = N'COLUMN', @level2name = N'RecordDeletedBy';

