--add default admin user
--username: admin@gmail.com
--password:Test@12345
INSERT INTO [dbo].[AdminUser] ([Name], [Email], [PasswordHash], [PasswordSalt], [RoleId], [IsActive], [ForgotPasswordKey], [ActivationKey]) VALUES ( N'Admin12', N'admin@gmail.com', N'm5jwr6TtGJEgDAXbEF0h+/do63xFG6y5pQbwe6Y33uiZ/gyYetdRgrxMFwjqeQug3oDO8BNYmVI7hy4Iwwwyzw==', N'4IppRwM3q953VbI8/glWhH6aPJ5tmpNRnZ3oV34FScU=', 1, 1, NULL, NULL)
