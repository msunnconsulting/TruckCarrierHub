
IF EXISTS (
    SELECT 1
    FROM TransportCompany c
    WHERE NOT EXISTS (
        SELECT 1
        FROM States s
        WHERE s.StateCode = c.PhysicalAddressStateCode
    )
)
BEGIN
    -- Raise an error
    RAISERROR('There are some StateCode in TransportCompany that do not exist in state.', 16, 1);
END
ELSE
BEGIN
-- Insert data into Cities table only if it does not already exist
    INSERT INTO Cities (CountryCode, StateCode, CityName, NumberOfCompanies)
    SELECT 
        States.CountryCode, 
        TransportCompany.PhysicalAddressStateCode, 
        PhysicalAddressCity, 
        COUNT(PhysicalAddressCity) as NumberOfCompanies
    FROM 
        TransportCompany
    LEFT JOIN 
        States ON States.StateCode = TransportCompany.PhysicalAddressStateCode
    GROUP BY 
        States.CountryCode, 
        TransportCompany.PhysicalAddressStateCode, 
        PhysicalAddressCity
    HAVING 
        NOT EXISTS (
            SELECT 1 
            FROM Cities 
            WHERE 
                Cities.CountryCode = States.CountryCode AND 
                Cities.StateCode = TransportCompany.PhysicalAddressStateCode AND 
                Cities.CityName = PhysicalAddressCity
        );
END