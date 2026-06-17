CREATE TABLE Cities (
    CountryCode NVARCHAR(5),
    StateCode NVARCHAR(255),
    CityName NVARCHAR(4000),
    NumberOfCompanies INT,
    Article TEXT NULL DEFAULT NULL,
    CityArticleAllowed BIT DEFAULT 0,

    CONSTRAINT UQ_Cities_StateCity UNIQUE (StateCode, CityName)
);