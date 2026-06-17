--add column into cargotype table
ALTER TABLE CargoType
  ADD CargoNameForUrl nvarchar(50);

--add column into servicetype table
ALTER TABLE [dbo].[ServiceType]
  ADD ServiceTypeForUrl nvarchar(50);