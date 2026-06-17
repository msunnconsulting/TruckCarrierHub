-- Set NowHiring = 1 where there is at least one matching record
UPDATE Business
SET NowHiring = 1
WHERE USDOTNumber IN (
    SELECT DISTINCT USDOTNumber FROM TransportCompany_DriverType
    UNION
    SELECT DISTINCT USDOTNumber FROM TransportCompany_TrailerType
);

-- Set NowHiring = 0 where there are NO matching records in either table
UPDATE Business
SET NowHiring = 0
WHERE USDOTNumber NOT IN (
    SELECT DISTINCT USDOTNumber FROM TransportCompany_DriverType
    UNION
    SELECT DISTINCT USDOTNumber FROM TransportCompany_TrailerType
);