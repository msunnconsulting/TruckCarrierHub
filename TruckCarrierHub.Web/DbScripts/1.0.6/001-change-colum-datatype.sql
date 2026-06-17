 --in transportCompany create nvarchar(255)
 ALTER TABLE TransportCompany ALTER COLUMN LegalName nvarchar(255);
 Go
 ALTER TABLE TransportCompany ALTER COLUMN PhysicalAddressCity nvarchar(255);
 Go

 ALTER TABLE TransportCompany ALTER COLUMN PhysicalAddressStateCode nvarchar(255);
 Go
 --in pretransport company table change datatype and remove column (MovedToTransportCompany)
 ALTER TABLE PreTransportCompany ALTER COLUMN NNDriversGrandTotalInterstateAndIntrastate int null;
 Go

 ALTER TABLE PreTransportCompany ALTER COLUMN IccDocketNumberFirst int null;
 Go

 --delete constraint without knowing its name
 declare @table_name nvarchar(256)
declare @col_name nvarchar(256)
declare @Command  nvarchar(1000)

set @table_name = N'PreTransportCompany'
set @col_name = N'MovedToTransportCompany'

select @Command = 'ALTER TABLE ' + @table_name + ' drop constraint ' + d.name
 from sys.tables t   
  join    sys.default_constraints d       
   on d.parent_object_id = t.object_id  
  join    sys.columns c      
   on c.object_id = t.object_id      
    and c.column_id = d.parent_column_id
 where t.name = @table_name
  and c.name = @col_name

execute (@Command);
 
 Go
 ALTER TABLE PreTransportCompany DROP COLUMN MovedToTransportCompany;
