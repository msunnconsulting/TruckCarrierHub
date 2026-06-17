-- Create index for hiring company. 
--get-state-which-have-atleast-one-hiring-company
CREATE NONCLUSTERED INDEX IX_PhysicalAddressStateCode
    ON TransportCompany ([PhysicalAddressStateCode]);   
GO
	