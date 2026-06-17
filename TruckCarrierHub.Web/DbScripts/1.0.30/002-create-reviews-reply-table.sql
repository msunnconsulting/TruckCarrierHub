CREATE TABLE [dbo].[ReviewReplies] (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    ReviewId     INT NOT NULL UNIQUE,
    CompanyUSDOT INT NOT NULL,
    ReplyText    NVARCHAR(1200) NOT NULL,
    CreatedDate  DATETIME NULL,
    UpdatedDate  DATETIME NULL,

    CONSTRAINT FK_ReviewReplies_Review
        FOREIGN KEY (ReviewId) REFERENCES [dbo].[Reviews] (Id)
        ON DELETE CASCADE,

    CONSTRAINT FK_ReviewReplies_Company
        FOREIGN KEY (CompanyUSDOT) REFERENCES [dbo].[TransportCompany] (USDOTNumber)        
);