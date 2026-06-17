EXEC sp_rename 'dbo.Main', 'TransportCompany'
GO 
--CREATE SYNONYM dbo.Main FOR TransportCompany
--GO

EXEC sp_rename 'dbo.PreMain', 'PreTransportCompany'
GO 
--CREATE SYNONYM dbo.PreMain FOR PreTransportCompany
--GO

EXEC sp_rename 'dbo.Main_CargoType', 'TransportCompany_CargoType'
GO 
--CREATE SYNONYM dbo.Main_CargoType FOR TransportCompany_CargoType
--GO

EXEC sp_rename 'dbo.Main_ServiceType', 'TransportCompany_ServiceType'
GO 
--CREATE SYNONYM dbo.Main_ServiceType FOR TransportCompany_ServiceType
--GO
