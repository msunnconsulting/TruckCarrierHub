--Insert default records in TrailerType table
SET IDENTITY_INSERT [dbo].[TrailerType] ON
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (1, N'Double/Triples', N'dt')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (2, N'Drop Deck', N'dd')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (3, N'Dry Van', N'dv')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (4, N'Flatbed', N'fb')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (5, N'HHG', N'hhg')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (6, N'Reefer', N'rf')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (7, N'Specialized', N'specialized')
INSERT INTO [dbo].[TrailerType] ([TrailerNumber], [TrailerName], [TrailerNameForUrl]) VALUES (8, N'Tanker', N't')
SET IDENTITY_INSERT [dbo].[TrailerType] OFF


--Insert default records in DriverType table
SET IDENTITY_INSERT [dbo].[DriverType] ON
INSERT INTO [dbo].[DriverType] ([DriverNumber], [DriverName], [DriverNameForUrl]) VALUES (1, N'Company Driver', N'cd')
INSERT INTO [dbo].[DriverType] ([DriverNumber], [DriverName], [DriverNameForUrl]) VALUES (2, N'Lease Purchase', N'lp')
INSERT INTO [dbo].[DriverType] ([DriverNumber], [DriverName], [DriverNameForUrl]) VALUES (3, N'Owner Operator', N'oo')
INSERT INTO [dbo].[DriverType] ([DriverNumber], [DriverName], [DriverNameForUrl]) VALUES (4, N'Team', N'tm')
SET IDENTITY_INSERT [dbo].[DriverType] OFF
