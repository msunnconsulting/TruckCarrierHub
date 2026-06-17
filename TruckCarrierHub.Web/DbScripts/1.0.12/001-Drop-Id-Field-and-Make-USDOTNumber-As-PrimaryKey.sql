declare @constraintName VARCHAR(50)
select @constraintName = CONSTRAINT_NAME  
FROM   INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE  TABLE_NAME = 'Business'  -- Table Name
       AND TABLE_SCHEMA = 'dbo'  -- change it if table is in some other schema 
       AND CONSTRAINT_TYPE = 'PRIMARY KEY'
	    
EXECUTE ('ALTER TABLE [dbo].[Business] DROP CONSTRAINT ' +@constraintName)

ALTER TABLE Business DROP COLUMN Id;
 
ALTER TABLE Business ADD CONSTRAINT PK_Business_USDOTNumber PRIMARY KEY (USDOTNumber)