--Add Company Name Column
ALTER TABLE TransportCompany
  ADD CompanyName nvarchar(255);
  Go

  --Set Company Name From Existing Records
  --here company name = businessName or legalname
UPDATE
    [TransportCompany]
SET 
  [Companyname] = CASE WHEN [DoingBusinessAsName] is null THEN [LegalName] ELSE [DoingBusinessAsName] END
  Go

  -- update datatype not null
	ALTER TABLE [TransportCompany] ALTER COLUMN [Companyname] nvarchar(255) NOT NULL
	Go


--	--here companyName = businessName(legalName)
--	UPDATE
--    [TransportCompany]
--SET 
--  [Companyname] = CASE WHEN [DoingBusinessAsName] is null THEN [LegalName] ELSE concat([DoingBusinessAsName],'(',[LegalName],')') END
--  Go
