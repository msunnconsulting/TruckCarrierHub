-- Frontend Side Tables
CREATE TABLE [dbo].[LoadType] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (255) NOT NULL,
    [Description] NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_LoadType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[LoadType] ON
INSERT INTO [dbo].[LoadType] ([Id], [Name], [Description]) VALUES (1, N'LTL', N'Less than Truckload')
INSERT INTO [dbo].[LoadType] ([Id], [Name], [Description]) VALUES (2, N'FTL/Rail', N'Full Truckload/Rail ')
INSERT INTO [dbo].[LoadType] ([Id], [Name], [Description]) VALUES (3, N'Flatbed', N'Flatbed/Open Deck')
INSERT INTO [dbo].[LoadType] ([Id], [Name], [Description]) VALUES (4, N'Container', N'Container')
SET IDENTITY_INSERT [dbo].[LoadType] OFF

CREATE TABLE [dbo].[LocationType] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [Name]       NVARCHAR (100) NOT NULL,
    [Location]   NVARCHAR (50)  NOT NULL,
    [LoadTypeId] INT            NULL,
    CONSTRAINT [PK_LocationType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoadType_LocationType_LoadTypeId] FOREIGN KEY ([LoadTypeId]) REFERENCES [dbo].[LoadType] ([Id])
);

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Possible Values: Pickup, Delivery', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'LocationType', @level2type = N'COLUMN', @level2name = N'Location';

SET IDENTITY_INSERT [dbo].[LocationType] ON
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (2, N'Business with Dock', N'Pickup', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (5, N'Business with no Dock', N'Pickup', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (6, N'Residential / Home Business', N'Pickup', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (7, N'Construction Site', N'Pickup', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (8, N'Limited Access', N'Pickup', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (9, N'Business with Dock', N'Delivery', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (10, N'Business with no Dock', N'Delivery', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (11, N'Residential / Home Business', N'Delivery', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (12, N'Construction Site', N'Delivery', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (13, N'Limited Access', N'Delivery', 1)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1009, N'Business with Dock', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1010, N'Business with no Dock', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1011, N'Residential / Home Business', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1012, N'Construction Site', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1013, N'Limited Access', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1014, N'Airport', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1015, N'Railroad Terminal', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1016, N'Trucking Terminal', N'Pickup', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1017, N'Business with Dock', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1018, N'Business with no Dock', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1019, N'Residential / Home Business', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1020, N'Construction Site', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1021, N'Limited Access', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1022, N'Airport', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1023, N'Railroad Terminal', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1024, N'Trucking Terminal', N'Delivery', 2)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1025, N'Business with Dock', N'Pickup', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1026, N'Business with no Dock', N'Pickup', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1027, N'Residential / Home Business', N'Pickup', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1028, N'Construction Site', N'Pickup', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1029, N'Limited Access', N'Pickup', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1030, N'Business with Dock', N'Delivery', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1031, N'Business with no Dock', N'Delivery', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1032, N'Residential / Home Business', N'Delivery', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1033, N'Construction Site', N'Delivery', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1034, N'Limited Access', N'Delivery', 3)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1035, N'Port', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1036, N'Rail Yard', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1037, N'Business with Dock', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1038, N'Business with no Dock', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1039, N'Residential / Home Business', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1041, N'Construction Site', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1042, N'Limited Access', N'Pickup', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1043, N'Port', N'Delivery', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1044, N'Rail Yard', N'Delivery', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1045, N'Business with Dock', N'Delivery', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1046, N'Business with no Dock', N'Delivery', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1047, N'Residential / Home Business', N'Delivery', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1048, N'Construction Site', N'Delivery', 4)
INSERT INTO [dbo].[LocationType] ([Id], [Name], [Location], [LoadTypeId]) VALUES (1049, N'Limited Access', N'Delivery', 4)
SET IDENTITY_INSERT [dbo].[LocationType] OFF


CREATE TABLE [dbo].[SpecialHandling] (
    [Id]             INT            IDENTITY (1, 1) NOT NULL,
    [Name]           NVARCHAR (100) NOT NULL,
    [Title]          NVARCHAR (255) NOT NULL,
    [LocationTypeId] INT            NULL,
    [LoadTypeId]     INT            NULL,
    CONSTRAINT [PK_SpecialHandling] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LocationType_SpecialHandling_LocationTypeId] FOREIGN KEY ([LocationTypeId]) REFERENCES [dbo].[LocationType] ([Id]),
    CONSTRAINT [FK_LoadType_SpecialHandling_LoadTypeId] FOREIGN KEY ([LoadTypeId]) REFERENCES [dbo].[LoadType] ([Id])
);

SET IDENTITY_INSERT [dbo].[SpecialHandling] ON
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1, N'Airport', N'Special pickup from Airport', 2, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2, N'Inside Pickup', N'For businesses, carrier pickups freight inside the building. For residential, carrier pickups freight in the front of the building.', 2, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (3, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 2, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (5, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 2, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (6, N'Trade Show', N'Usually requires appointments and returning the shipment to the pick up location', 2, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (9, N'Airport', N'Special pickup from Airport', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (10, N'Inside Pickup', N'For businesses, carrier pickups freight inside the building. For residential, carrier pickups freight in the front of the building.', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (12, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (13, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (19, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (20, N'Trade Show', N'Usually requires appointments and returning the shipment to the pick up location', 5, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (21, N'Airport', N'Special pickup from Airport', 6, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (22, N'Inside Pickup', N'For businesses, carrier pickups freight inside the building. For residential, carrier pickups freight in the front of the building.', 6, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (23, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 6, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (24, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 6, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (26, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 6, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (27, N'Airport', N'Special pickup from Airport', 7, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (30, N'Inside Pickup', N'For businesses, carrier pickups freight inside the building. For residential, carrier pickups freight in the front of the building.', 7, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (31, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 7, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (33, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 7, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (36, N'Straight Truck', N'Usually requires appointments and returning the shipment to the pick up location', 7, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (37, N'Airport', N'Special pickup from Airport', 8, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (38, N'Inside Pickup', N'For businesses, carrier pickups freight inside the building. For residential, carrier pickups freight in the front of the building.', 8, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (39, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 8, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (40, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 8, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (42, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 8, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1002, N'Airport', N'Special delivery to Airport', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1003, N'Amazon', N'Special delivery to Amazon warehouse', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1004, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1005, N'Inside Delivery', N'For businesses, carrier delivers freight inside the building. For residential, carrier delivers freight to the front of the building.', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1006, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1007, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1008, N'Trade Show', N'Usually requires appointments and returning the shipment to the pick up location', 9, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1009, N'Airport', N'Special delivery to Airport', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1010, N'Amazon', N'Special delivery to Amazon warehouse', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1011, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1012, N'Inside Delivery', N'For businesses, carrier delivers freight inside the building. For residential, carrier delivers freight to the front of the building.', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1013, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1014, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1015, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1016, N'Trade Show', N'Usually requires appointments and returning the shipment to the pick up location', 10, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1017, N'Airport', N'Special delivery to Airport', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1018, N'Amazon', N'Special delivery to Amazon warehouse', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1019, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1020, N'Inside Delivery', N'For businesses, carrier delivers freight inside the building. For residential, carrier delivers freight to the front of the building.', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1021, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1022, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1023, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 11, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1024, N'Airport', N'Special delivery to Airport', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1025, N'Amazon', N'Special delivery to Amazon warehouse', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1026, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1027, N'Inside Delivery', N'For businesses, carrier delivers freight inside the building. For residential, carrier delivers freight to the front of the building.', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1028, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1029, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1030, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 12, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1031, N'Airport', N'Special delivery to Airport', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1032, N'Amazon', N'Special delivery to Amazon warehouse', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1033, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1034, N'Inside Delivery', N'For businesses, carrier delivers freight inside the building. For residential, carrier delivers freight to the front of the building.', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1035, N'Jigger / Pallet Jack', N'A tool used for lifting and moving pallets', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1036, N'Tailgate / Liftgate', N'A platform at the rear of the truck, used for loading and unloading heavy freight at locations without docks or forklift', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (1037, N'Straight Truck', N'A truck that can fit into narrow streets, e.g. streets in residential areas. Location like School, College, Farm, Home Business or Winery may require a straight truck', 13, 1)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2004, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1009, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2006, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1010, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2007, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1011, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2008, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1012, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2009, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1013, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2010, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1017, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2012, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1018, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2013, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1019, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2014, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1020, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2015, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1021, 2)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2016, N'Tarping', N'Covering of the flatbed load to protect it from elements', 1025, 3)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2017, N'Tarping', N'Covering of the flatbed load to protect it from elements', 1026, 3)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2018, N'Tarping', N'Covering of the flatbed load to protect it from elements', 1027, 3)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2019, N'Tarping', N'Covering of the flatbed load to protect it from elements', 1028, 3)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2020, N'Tarping', N'Covering of the flatbed load to protect it from elements', 1029, 3)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2021, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1035, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2022, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1036, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2023, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1037, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2025, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1038, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2026, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1039, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2027, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1041, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2029, N'Pickup by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1042, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2031, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1043, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2033, N'Return of Container', N'', 1043, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2034, N'Live Unload', N'', 1043, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2035, N'Self Unloading', N'', 1043, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2036, N'Loading', N'', 1043, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2037, N'Winch Truck/ Crane Truck', N'', 1043, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2038, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1044, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2039, N'Return of Container', N'', 1044, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2040, N'Live Unload', N'', 1044, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2041, N'Self Unloading', N'', 1044, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2042, N'Loading', N'', 1044, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2043, N'Winch Truck/ Crane Truck', N'', 1044, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2045, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1045, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2047, N'Return of Container', N'', 1045, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2048, N'Live Unload', N'', 1045, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2049, N'Self Unloading', N'', 1045, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2050, N'Loading', N'', 1045, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2051, N'Winch Truck/ Crane Truck', N'', 1045, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2052, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1046, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2054, N'Return of Container', N'', 1046, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2055, N'Live Unload', N'', 1046, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2056, N'Self Unloading', N'', 1046, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2057, N'Loading', N'', 1046, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2058, N'Winch Truck/ Crane Truck', N'', 1046, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2059, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1047, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2061, N'Return of Container', N'', 1047, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2062, N'Live Unload', N'', 1047, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2063, N'Self Unloading', N'', 1047, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2064, N'Loading', N'', 1047, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2065, N'Winch Truck/ Crane Truck', N'', 1047, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2066, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1048, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2068, N'Return of Container', N'', 1048, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2069, N'Live Unload', N'', 1048, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2070, N'Self Unloading', N'', 1048, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2071, N'Loading', N'', 1048, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2072, N'Winch Truck/ Crane Truck', N'', 1048, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2073, N'Delivery by Appointment', N'A request for an exact date and time or a window for pick up of 2 hours or less', 1049, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2075, N'Return of Container', N'', 1049, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2076, N'Live Unload', N'', 1049, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2077, N'Self Unloading', N'', 1049, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2078, N'Loading', N'', 1049, 4)
INSERT INTO [dbo].[SpecialHandling] ([Id], [Name], [Title], [LocationTypeId], [LoadTypeId]) VALUES (2079, N'Winch Truck/ Crane Truck', N'', 1049, 4)
SET IDENTITY_INSERT [dbo].[SpecialHandling] OFF

CREATE TABLE [dbo].[QuoteTemperature] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [TemperatureType] NVARCHAR (10) NULL,
    CONSTRAINT [PK_QuoteTemperature] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[QuoteTemperature] ON
INSERT INTO [dbo].[QuoteTemperature] ([Id], [TemperatureType]) VALUES (1, N'°C')
INSERT INTO [dbo].[QuoteTemperature] ([Id], [TemperatureType]) VALUES (2, N'°F')
SET IDENTITY_INSERT [dbo].[QuoteTemperature] OFF

CREATE TABLE [dbo].[QuoteRefrigeration] (
    [Id]                INT           IDENTITY (1, 1) NOT NULL,
    [RefrigerationType] NVARCHAR (50) NULL,
    [LoadTypeId]        INT           NULL,
    CONSTRAINT [PK_QuoteRefrigeration] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoadType_QuoteRefrigeration_LoadTypeId] FOREIGN KEY ([LoadTypeId]) REFERENCES [dbo].[LoadType] ([Id])
);

SET IDENTITY_INSERT [dbo].[QuoteRefrigeration] ON
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (1, N'Keep warm', 2)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (2, N'Keep cool', 2)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (3, N'Protect from freeze', 2)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (4, N'Keep frozen', 2)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (5, N'Exact temperature', 2)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (6, N'Keep warm', 1)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (7, N'Keep cool', 1)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (8, N'Protect from freeze', 1)
INSERT INTO [dbo].[QuoteRefrigeration] ([Id], [RefrigerationType], [LoadTypeId]) VALUES (9, N'Keep frozen', 1)
SET IDENTITY_INSERT [dbo].[QuoteRefrigeration] OFF

CREATE TABLE [dbo].[LoadTruckType] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [TruckType]  NVARCHAR (50) NOT NULL,
    [LoadTypeID] INT           NOT NULL,
    CONSTRAINT [PK_LoadTruckType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoadType_LoadTruckType_LoadTypeId] FOREIGN KEY ([LoadTypeID]) REFERENCES [dbo].[LoadType] ([Id])
);

SET IDENTITY_INSERT [dbo].[LoadTruckType] ON
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (1, N'FTL (Road)', 2)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (2, N'Rail', 2)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (3, N'Either', 2)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (4, N'Flatbed', 3)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (5, N'Roll Tite / Conestoga', 3)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (6, N'Super B', 3)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (7, N'Low Bed / RGN', 3)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (8, N'China Top', 3)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (9, N'Step Deck', 3)
INSERT INTO [dbo].[LoadTruckType] ([Id], [TruckType], [LoadTypeID]) VALUES (10, N'Other', 3)
SET IDENTITY_INSERT [dbo].[LoadTruckType] OFF

CREATE TABLE [dbo].[LoadItemType] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [LoadItemType] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_LoadItemType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[LoadItemType] ON
INSERT INTO [dbo].[LoadItemType] ([Id], [LoadItemType]) VALUES (1, N'Pallets')
INSERT INTO [dbo].[LoadItemType] ([Id], [LoadItemType]) VALUES (2, N'Pieces')
INSERT INTO [dbo].[LoadItemType] ([Id], [LoadItemType]) VALUES (3, N'Boxes')
INSERT INTO [dbo].[LoadItemType] ([Id], [LoadItemType]) VALUES (4, N'Bundles')
INSERT INTO [dbo].[LoadItemType] ([Id], [LoadItemType]) VALUES (5, N'Crates')
INSERT INTO [dbo].[LoadItemType] ([Id], [LoadItemType]) VALUES (6, N'Totes')
SET IDENTITY_INSERT [dbo].[LoadItemType] OFF

CREATE TABLE [dbo].[LoadInfo] (
    [Id]           INT          IDENTITY (1, 1) NOT NULL,
    [LoadInfoType] VARCHAR (50) NULL,
    CONSTRAINT [PK_LoadInfo] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[LoadInfo] ON
INSERT INTO [dbo].[LoadInfo] ([Id], [LoadInfoType]) VALUES (1, N'Palletized')
INSERT INTO [dbo].[LoadInfo] ([Id], [LoadInfoType]) VALUES (2, N'Floorloaded')
SET IDENTITY_INSERT [dbo].[LoadInfo] OFF

CREATE TABLE [dbo].[LoadContainerType] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [StatusType] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_LoadStatusType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[LoadContainerType] ON
INSERT INTO [dbo].[LoadContainerType] ([Id], [StatusType]) VALUES (1, N'Empty')
INSERT INTO [dbo].[LoadContainerType] ([Id], [StatusType]) VALUES (2, N'Loaded')
INSERT INTO [dbo].[LoadContainerType] ([Id], [StatusType]) VALUES (3, N'Modified')
SET IDENTITY_INSERT [dbo].[LoadContainerType] OFF

CREATE TABLE [dbo].[LoadContainerLength] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [LengthOfContainer] NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_LoadContainerType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[LoadContainerLength] ON
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (1, N'6'', 8'', 9'' High Cube')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (2, N'20'' Standard')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (3, N'20'' Double Door')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (4, N'20'' High Cube')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (5, N'40'' Standard')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (6, N'40'' Double Door')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (7, N'40'' High Cube')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (8, N'45'' Standard')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (9, N'45'' Double Door')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (10, N'45'' High Cube')
INSERT INTO [dbo].[LoadContainerLength] ([Id], [LengthOfContainer]) VALUES (11, N'53'' High Cube')
SET IDENTITY_INSERT [dbo].[LoadContainerLength] OFF

CREATE TABLE [dbo].[LoadClass] (
    [Id]   INT           IDENTITY (1, 1) NOT NULL,
    [Name] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_LoadClass] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[LoadClass] ON
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (1, N'50')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (2, N'55')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (3, N'60')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (4, N'65')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (5, N'70')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (6, N'77.5')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (7, N'85')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (8, N'92.5')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (9, N'100')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (10, N'110')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (11, N'125')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (12, N'150')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (13, N'175')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (14, N'200')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (15, N'250')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (16, N'300')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (17, N'400')
INSERT INTO [dbo].[LoadClass] ([Id], [Name]) VALUES (18, N'500')
SET IDENTITY_INSERT [dbo].[LoadClass] OFF

CREATE TABLE [dbo].[Load] (
    [Id]                  INT            IDENTITY (1, 1) NOT NULL,
    [GoodsDescription]    NVARCHAR (255) NOT NULL,
    [LoadStatusTypeId]    INT            NULL,
    [NoOfContainers]      INT            NULL,
    [LoadContainerTypeId] INT            NULL,
    [NoOfItems]           INT            NULL,
    [LoadItemTypeId]      INT            NULL,
    [DimentionLength]     INT            NULL,
    [DimentionWidth]      INT            NULL,
    [DimentionHeight]     INT            NULL,
    [TotalWeight]         INT            NULL,
    [LoadTruckTypeId]     INT            NULL,
    [LoadInfoId]          INT            NULL,
    [LoadClassId]         INT            NULL,
    [IsHasmat?]           BIT            NULL,
    [IsNonStackable?]     BIT            NULL,
    CONSTRAINT [PK_Load] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoadTruckType_Load_LoadTruckTypeId] FOREIGN KEY ([LoadTruckTypeId]) REFERENCES [dbo].[LoadTruckType] ([Id]),
    CONSTRAINT [FK_LoadInfo_Load_LoadInfoId] FOREIGN KEY ([LoadInfoId]) REFERENCES [dbo].[LoadInfo] ([Id]),
    CONSTRAINT [FK_LoadItemType_Load_LoadItemTypeId] FOREIGN KEY ([LoadItemTypeId]) REFERENCES [dbo].[LoadItemType] ([Id]),
    CONSTRAINT [FK_LoadClass_Load_LoadClassId] FOREIGN KEY ([LoadClassId]) REFERENCES [dbo].[LoadClass] ([Id]),
    CONSTRAINT [FK_LoadStatusType_Load_LoadStatusTypeId] FOREIGN KEY ([LoadStatusTypeId]) REFERENCES [dbo].[LoadContainerType] ([Id]),
    CONSTRAINT [FK_LoadContainerType_Load_LoadContainerTypeId] FOREIGN KEY ([LoadContainerTypeId]) REFERENCES [dbo].[LoadContainerLength] ([Id])
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Used to store Total Weight or Weight or Total Weight Per Container or Weight Per Item', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'Load', @level2type = N'COLUMN', @level2name = N'TotalWeight';

CREATE TABLE [dbo].[Quote] (
    [Id]                    INT            IDENTITY (1, 1) NOT NULL,
    [CreatedDate]           DATETIME       NOT NULL,
    [OriginURL]             NVARCHAR (255) NOT NULL,
    [ShipperFirstName]      NVARCHAR (255) NOT NULL,
    [ShipperLastName]       NVARCHAR (255) NOT NULL,
    [ShipperEmail]          NVARCHAR (255) NOT NULL,
    [ShipperPhone]          NVARCHAR (255) NOT NULL,
    [ShipperCompanyName]    NVARCHAR (255) NULL,
    [FromState]             NVARCHAR (255) NOT NULL,
    [FromCity]              NVARCHAR (255) NOT NULL,
    [ToState]               NVARCHAR (255) NOT NULL,
    [ToCity]                NVARCHAR (255) NOT NULL,
    [LoadTypeId]            INT            NOT NULL,
    [PickupDate]            DATETIME       NOT NULL,
    [RefrigerationId]       INT            NULL,
    [TemperatureId]         INT            NULL,
    [Temperature]           DECIMAL (3)    NULL,
    [FromLocationTypeId]    INT            NULL,
    [ToLocationTypeId]      INT            NULL,
    [LoadDetailDescription] NVARCHAR (600) NULL,
    [IsFlexible]            BIT            NULL,
    CONSTRAINT [PK_Quote] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoadType_Quote_LoadTypeId] FOREIGN KEY ([LoadTypeId]) REFERENCES [dbo].[LoadType] ([Id]),
    CONSTRAINT [FK_LocationType_Quote_FromLocationTypeId] FOREIGN KEY ([FromLocationTypeId]) REFERENCES [dbo].[LocationType] ([Id]),
    CONSTRAINT [FK_LocationType_Quote_ToLocationTypeId] FOREIGN KEY ([ToLocationTypeId]) REFERENCES [dbo].[LocationType] ([Id]),
    CONSTRAINT [FK_QuoteRefrigeration_Quote_RefrigerationId] FOREIGN KEY ([RefrigerationId]) REFERENCES [dbo].[QuoteRefrigeration] ([Id]),
    CONSTRAINT [FK_QuoteTemperature_Quote_TemperatureId] FOREIGN KEY ([TemperatureId]) REFERENCES [dbo].[QuoteTemperature] ([Id])
);

CREATE TABLE [dbo].[Quote_SpecialHandling_Location] (
    [Id]                INT IDENTITY (1, 1) NOT NULL,
    [QuoteId]           INT NOT NULL,
    [SpecialHandlingId] INT NOT NULL,
    CONSTRAINT [PK_Quote_SpecialHandling_Location] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Quote_Quote_SpecialHandling_Location_QuoteId] FOREIGN KEY ([QuoteId]) REFERENCES [dbo].[Quote] ([Id])
);


-- Admin Side Tables
CREATE TABLE [dbo].[Carriers] (
    [Id]                INT            IDENTITY (1, 1) NOT NULL,
    [USDOTNumber]       INT            NULL,
    [CompanyName]       NVARCHAR (255) NOT NULL,
    [ContactPerson1]    NVARCHAR (255) NOT NULL,
    [ContactEmail1]     NVARCHAR (255) NOT NULL,
    [ContactPhone1]     NVARCHAR (255) NOT NULL,
    [ContactPerson2]    NVARCHAR (255) NOT NULL,
    [ContactEmail2]     NVARCHAR (255) NOT NULL,
    [ContactPhone2]     NVARCHAR (255) NOT NULL,
    [CarrierActive]     BIT            NOT NULL,
    [Website]           NVARCHAR (255) NULL,
    [MaxQuotesPerMonth] INT            NULL,
    CONSTRAINT [PK_Carriers] PRIMARY KEY CLUSTERED ([Id] ASC)
);

CREATE TABLE [dbo].[Carrier_LoadType] (
    [CarrierId]  INT NOT NULL,
    [LoadTypeID] INT NOT NULL,
    [Id]         INT IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_Carrier_LoadType] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_LoadType_Carrier_LoadType_LoadTypeId] FOREIGN KEY ([LoadTypeID]) REFERENCES [dbo].[LoadType] ([Id]),
    CONSTRAINT [FK_Carriers_Carrier_LoadType_CarrierId] FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers] ([Id])
);

CREATE TABLE [dbo].[Carrier_State_From] (
    [CarrierId] INT          NOT NULL,
    [StateCode] NVARCHAR (5) NULL,
    [Id]        INT          IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_Carrier_State_From] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Carriers_Carrier_State_From_CarrierId] FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers] ([Id])
);

CREATE TABLE [dbo].[Carrier_State_To] (
    [CarrierId] INT          NOT NULL,
    [StateCode] NVARCHAR (5) NULL,
    [Id]        INT          IDENTITY (1, 1) NOT NULL,
    CONSTRAINT [PK_Carrier_State_To] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Carriers_Carrier_State_To_CarrierId] FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers] ([Id])
);

CREATE TABLE [dbo].[GetAQuoteToShow] (
    [Id]            INT            IDENTITY (1, 1) NOT NULL,
    [Name]          NVARCHAR (255) NOT NULL,
    [ControlToShow] BIT            NOT NULL,
    CONSTRAINT [PK_GetAQuoteToShow] PRIMARY KEY CLUSTERED ([Id] ASC)
);

SET IDENTITY_INSERT [dbo].[GetAQuoteToShow] ON
INSERT INTO [dbo].[GetAQuoteToShow] ([Id], [Name], [ControlToShow]) VALUES (1, N'Show on homepage', 1)
INSERT INTO [dbo].[GetAQuoteToShow] ([Id], [Name], [ControlToShow]) VALUES (2, N'Show on State/Province page', 0)
INSERT INTO [dbo].[GetAQuoteToShow] ([Id], [Name], [ControlToShow]) VALUES (3, N'Show on City page', 1)
INSERT INTO [dbo].[GetAQuoteToShow] ([Id], [Name], [ControlToShow]) VALUES (4, N'Show on Company page', 0)
SET IDENTITY_INSERT [dbo].[GetAQuoteToShow] OFF


CREATE TABLE [dbo].[QuoteSent] (
    [Id]        INT      IDENTITY (1, 1) NOT NULL,
    [QuoteID]   INT      NOT NULL,
    [CarrierId] INT      NOT NULL,
    [SentDate]  DATETIME NOT NULL,
    CONSTRAINT [PK_QuoteSent] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Quote_QuoteSent_QuoteId] FOREIGN KEY ([QuoteID]) REFERENCES [dbo].[Quote] ([Id]),
    CONSTRAINT [FK_Carrier_QuoteSent_CarrierId] FOREIGN KEY ([CarrierId]) REFERENCES [dbo].[Carriers] ([Id])
);
