CREATE NONCLUSTERED INDEX IX_Latitude
    ON TransportCompany (Latitude);   
GO  

CREATE NONCLUSTERED INDEX IX_Longitude
    ON TransportCompany (Longitude);   
GO  


CREATE NONCLUSTERED INDEX IX_Status
    ON TransportCompany ([Status]);   
GO

CREATE NONCLUSTERED INDEX IX_USDOTNUMBER
    ON DeletedRecords ([USDOTNUMBER]);   
GO



CREATE NONCLUSTERED INDEX IX_DateLastChanged
    ON PreTransportCompany ([DateLastChanged]);   
GO

CREATE NONCLUSTERED INDEX IX_DateLastChanged
    ON TransportCompany ([DateLastChanged]);   
GO
	