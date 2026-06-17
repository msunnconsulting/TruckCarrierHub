CREATE TABLE [dbo].[Login] (
    [Id]                BIGINT           IDENTITY (1, 1) NOT NULL,
    [Name]              NVARCHAR (100)   NOT NULL,
    [Email]             NVARCHAR (256)   NOT NULL,
    [PasswordHash]      VARCHAR (256)    NOT NULL,
    [PasswordSalt]      VARCHAR (64)     NOT NULL,
    [RoleId]            TINYINT          NOT NULL,
    [IsActive]          BIT              NOT NULL,
    [ForgotPasswordKey] VARCHAR (50)     NULL,
    [ActivationKey]     UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);

