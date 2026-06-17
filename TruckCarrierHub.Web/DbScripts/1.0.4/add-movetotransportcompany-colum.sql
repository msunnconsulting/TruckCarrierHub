
--Add Column=MovedToTransportCompany which is by default false
--when user update record it will be true for which record is updated in main table
ALTER TABLE PreTransportCompany ADD MovedToTransportCompany BIT NOT NULL DEFAULT 0;