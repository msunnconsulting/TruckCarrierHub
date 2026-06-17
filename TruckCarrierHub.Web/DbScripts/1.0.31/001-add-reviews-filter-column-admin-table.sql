ALTER TABLE [dbo].[Admin] ADD ReviewsFilter INT;
GO

Update Admin SET ReviewsFilter = 0;
GO