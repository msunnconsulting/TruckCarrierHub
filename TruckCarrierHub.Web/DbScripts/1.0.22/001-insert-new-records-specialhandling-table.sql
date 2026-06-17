-- Insert new record for LTL Screen "Pickup by Appointment" checkbox option is not available to LTL load type.
SET IDENTITY_INSERT [dbo].[SpecialHandling] ON
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (3004, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 2, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (3006, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (3009, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 6, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (3011, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 7, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (3012, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 8, 1)
SET IDENTITY_INSERT [dbo].[SpecialHandling] OFF
