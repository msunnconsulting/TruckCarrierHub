--Add primary key in Transport_CargoType table
ALTER TABLE TransportCompany_CargoType ADD PRIMARY KEY(USDOTNumber,CargoNumber);
--Add Primary key in Transport_ServiceType table
ALTER TABLE TransportCompany_ServiceType ADD PRIMARY KEY(USDOTNumber,ServiceTypeNumber);