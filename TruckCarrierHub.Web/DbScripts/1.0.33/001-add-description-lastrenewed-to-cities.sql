-- Add Description and LastRenewedDate to Cities.
-- Description replaces the CityArticleAllowed flag as the signal that
-- city content exists; Article null-check replaces it for body content.
-- Wipe Article on all existing rows — old content had quality issues
-- and will be regenerated via the new Renew process.

ALTER TABLE Cities ADD Description NVARCHAR(500) NULL;
ALTER TABLE Cities ADD LastRenewedDate DATETIME NULL;
UPDATE Cities SET Article = NULL;
