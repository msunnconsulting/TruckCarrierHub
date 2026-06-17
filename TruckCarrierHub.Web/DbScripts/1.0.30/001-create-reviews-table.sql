CREATE TABLE [dbo].[Reviews] (
    Id             INT IDENTITY(1,1) PRIMARY KEY,
    CompanyUSDOT   INT NOT NULL,
    ReviewerUSDOT  INT NOT NULL,
    Rating         INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Comment        NVARCHAR(1200) NULL,
    CreatedDate    DATETIME NULL,
    UpdatedDate    DATETIME NULL,
	
    CONSTRAINT FK_Reviews_Company
        FOREIGN KEY (CompanyUSDOT) REFERENCES [dbo].[TransportCompany] (USDOTNumber)
        ON DELETE CASCADE,
        
    CONSTRAINT FK_Reviews_Reviewer
        FOREIGN KEY (ReviewerUSDOT) REFERENCES [dbo].[Business] (USDOTNumber)
        ON DELETE CASCADE,
    
);