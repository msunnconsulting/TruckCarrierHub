
GO
CREATE TABLE [dbo].[Admin] (
    [HomePageTitle]        NVARCHAR (MAX) NULL,
    [HomePageDescription]  NVARCHAR (MAX) NULL,
    [StatePageTitle]       NVARCHAR (MAX) NULL,
    [StatePageDescription] NVARCHAR (MAX) NULL,
    [CityPageTitle]        NVARCHAR (MAX) NULL,
    [CityPageDescription]  NVARCHAR (MAX) NULL
);

GO
CREATE TABLE [dbo].[CargoType] (
    [CargoNumber] INT            IDENTITY (1, 1) NOT NULL,
    [CargoName]   NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_CargoType] PRIMARY KEY CLUSTERED ([CargoNumber] ASC)
);

GO
CREATE TABLE [dbo].[DeletedRecords] (
    [USDOTNumber]     INT           NOT NULL,
    [RecordDeletedBy] INT           NOT NULL,
    [DeletedOn]       SMALLDATETIME NOT NULL,
    CONSTRAINT [PK_DeletedRecords] PRIMARY KEY CLUSTERED ([USDOTNumber] ASC)
);

GO
CREATE TABLE [dbo].[Main] (
    [Status]                                      NVARCHAR (255) NULL,
    [EntityType]                                  NVARCHAR (255) NULL,
    [USDOTNumber]                                 INT            NOT NULL,
    [LegalName]                                   NVARCHAR (100) NULL,
    [DoingBusinessAsName]                         NVARCHAR (255) NULL,
    [DunBradstreetNumber]                         NVARCHAR (255) NULL,
    [PhysicalAddressNationality]                  NVARCHAR (20)  NULL,
    [PhysicalAddressFmcsaRegion]                  INT            NULL,
    [PhysicalAddressStreet]                       NVARCHAR (255) NULL,
    [PhysicalAddressCity]                         NVARCHAR (100) NULL,
    [PhysicalAddressCountyCode]                   INT            NULL,
    [PhysicalAddressStateCode]                    NVARCHAR (4)   NULL,
    [PhysicalAddressZipCode]                      NVARCHAR (255) NULL,
    [UndeliverablePhysicalAddress]                NVARCHAR (255) NULL,
    [OfficeTelephoneNumber]                       NVARCHAR (255) NULL,
    [CellPhoneNumber]                             NVARCHAR (255) NULL,
    [OfficeFaxPhoneNumber]                        NVARCHAR (255) NULL,
    [MailingAddressNationality]                   NVARCHAR (255) NULL,
    [MailingAddressStreet]                        NVARCHAR (255) NULL,
    [MailingAddressCity]                          NVARCHAR (255) NULL,
    [MailingAddressCountyCode]                    INT            NULL,
    [MailingAddressStateCode]                     NVARCHAR (255) NULL,
    [MailingAddressZipCode]                       NVARCHAR (255) NULL,
    [UndeliverableMailingAddress]                 NVARCHAR (255) NULL,
    [OfficerInChargeCode]                         INT            NULL,
    [SafetyInvestigatorTerritoryCode]             NVARCHAR (255) NULL,
    [IccDocketNumber1Prefix]                      NVARCHAR (255) NULL,
    [IccDocketNumberFirst]                        INT            NULL,
    [IccDocket2Prefix]                            NVARCHAR (255) NULL,
    [IccDocketNumberSecond]                       NVARCHAR (255) NULL,
    [IccDocket3Prefix]                            NVARCHAR (255) NULL,
    [IccDocketNumberThird]                        NVARCHAR (255) NULL,
    [Classification]                              NVARCHAR (255) NULL,
    [ClassificationOtherDefined]                  NVARCHAR (255) NULL,
    [OperationCarrierInterstate]                  NVARCHAR (255) NULL,
    [OperationCarrierIntrastateHazmat]            NVARCHAR (255) NULL,
    [OperationCarrierIntrastateNonHazmat]         NVARCHAR (255) NULL,
    [OperationShipperInterstate]                  NVARCHAR (255) NULL,
    [OperationShipperIntrastate]                  NVARCHAR (255) NULL,
    [OperationVehicleRegistrant]                  NVARCHAR (255) NULL,
    [BusinessOrganizationCode]                    NVARCHAR (255) NULL,
    [CargoTransportedAGeneralFreight]             NVARCHAR (255) NULL,
    [CargoTransportedBHouseholdGoods]             NVARCHAR (255) NULL,
    [CargoTransportedCMetalSheetsCoilsRolls]      NVARCHAR (255) NULL,
    [CargoTransportedDMotorVehicles]              NVARCHAR (255) NULL,
    [CargoTransportedEDriveawayTowaway]           NVARCHAR (255) NULL,
    [CargoTransportedFLogsPolesBeamsLumber]       NVARCHAR (255) NULL,
    [CargoTransportedGBuildingMaterials]          NVARCHAR (255) NULL,
    [CargoTransportedHMobileHomes]                NVARCHAR (255) NULL,
    [CargoTransportedIMachineryLargeObjects]      NVARCHAR (255) NULL,
    [CargoTransportedJFreshProduce]               NVARCHAR (255) NULL,
    [CargoTransportedKLiquidsGases]               NVARCHAR (255) NULL,
    [CargoTransportedLIintermodalContainers]      NVARCHAR (255) NULL,
    [CargoTransportedMPassengers]                 NVARCHAR (255) NULL,
    [CargoTransportedNOilfieldEquipment]          NVARCHAR (255) NULL,
    [CargoTransportedOLivestock]                  NVARCHAR (255) NULL,
    [CargoTransportedPGrainFeedHay]               NVARCHAR (255) NULL,
    [CargoTransportedQCoalCoke]                   NVARCHAR (255) NULL,
    [CargoTransportedRMeat]                       NVARCHAR (255) NULL,
    [CargoTransportedSGarbageRefuseTrash]         NVARCHAR (255) NULL,
    [CargoTransportedTUSMail]                     NVARCHAR (255) NULL,
    [CargoTransportedUChemicals]                  NVARCHAR (255) NULL,
    [CargoTransportedVCommoditiesDryBulk]         NVARCHAR (255) NULL,
    [CargoTransportedWRefrigeratedFood]           NVARCHAR (255) NULL,
    [CargoTransportedXBeverages]                  NVARCHAR (255) NULL,
    [CargoTransportedYPaperProducts]              NVARCHAR (255) NULL,
    [CargoTransportedZUtility]                    NVARCHAR (255) NULL,
    [CargoTransportedAAFarmSupplies]              NVARCHAR (255) NULL,
    [CargoTransportedBBConstruction]              NVARCHAR (255) NULL,
    [CargoTransportedCCWaterWell]                 NVARCHAR (255) NULL,
    [CargoTransportedDDOther]                     NVARCHAR (255) NULL,
    [CargoTransportedOtherDefined]                NVARCHAR (255) NULL,
    [HazmatIndicator]                             NVARCHAR (255) NULL,
    [NNEquipmentUnitsOwnedTruck]                  INT            NULL,
    [NNEquipmentUnitsOwnedTractor]                INT            NULL,
    [NNEquipmentUnitsOwnedTrailer]                INT            NULL,
    [NNEquipmentUnitsOwnedMotorCoach]             INT            NULL,
    [NNEquipmentUnitsOwnedSchoolBus1_8]           INT            NULL,
    [NNEquipmentUnitsOwnedSchoolBus9_15]          INT            NULL,
    [NNEquipmentUnitsOwnedSchoolBus16Plus]        INT            NULL,
    [NNEquipmentUnitsOwnedMiniBusVan16Plus]       INT            NULL,
    [NNEquipmentUnitsOwnedMiniBusVan1_8]          INT            NULL,
    [NNEquipmentUnitsOwnedMiniBusVan9_15]         INT            NULL,
    [NNEquipmentUnitsOwnedLimo1_8]                INT            NULL,
    [NNEquipmentUnitsOwnedLimo9_15]               INT            NULL,
    [NNEquipmentUnitsOwnedLimo16Plus]             INT            NULL,
    [NNEquipmentUnitsTermLeasedTruck]             INT            NULL,
    [NNEquipmentUnitsTermLeasedTractor]           INT            NULL,
    [NNEquipmentUnitsTermLeasedTrailer]           INT            NULL,
    [NNEquipmentUnitsTermLeasedMotorCoach]        INT            NULL,
    [NNEquipmentUnitsTermLeasedSchoolBus1_8]      INT            NULL,
    [NNEquipmentUnitsTermLeasedSchoolBus9_15]     INT            NULL,
    [NNEquipmentUnitsTermLeasedSchoolBus16Plus]   INT            NULL,
    [NNEquipmentUnitsTermLeasedMiniBusVan16Plus]  INT            NULL,
    [NNEquipmentUnitsTermLeasedMiniBusVan1_8]     INT            NULL,
    [NNEquipmentUnitsTermLeasedMiniBusVan9_15]    INT            NULL,
    [NNEquipmentUnitsTermLeasedLimo1_8]           INT            NULL,
    [NNEquipmentUnitsTermLeasedLimo9_15]          INT            NULL,
    [NNEquipmentUnitsTermLeasedLimo16Plus]        INT            NULL,
    [NNEquipmentUnitsTripLeasedTruck]             INT            NULL,
    [NNEquipmentUnitsTripLeasedTractor]           INT            NULL,
    [NNEquipmentUnitsTripLeasedTrailer]           INT            NULL,
    [NNEquipmentUnitsTripLeasedMotorCoach]        INT            NULL,
    [NNEquipmentUnitsTripLeasedSchoolBus1_8]      INT            NULL,
    [NNEquipmentUnitsTripLeasedSchoolBus9_15]     INT            NULL,
    [NNEquipmentUnitsTripLeasedSchoolBus16Plus]   INT            NULL,
    [NNEquipmentUnitsTripLeasedMiniBusVan16Plus]  INT            NULL,
    [NNEquipmentUnitsTripLeasedMiniBusVan1_8]     INT            NULL,
    [NNEquipmentUnitsTripLeasedMiniBusVan9_15]    INT            NULL,
    [NNEquipmentUnitsTripLeasedLimo1_8]           INT            NULL,
    [NNEquipmentUnitsTripLeasedLimo9_15]          INT            NULL,
    [NNEquipmentUnitsTripLeasedLimo16Plus]        INT            NULL,
    [TotalNumberOfTrucks]                         INT            NULL,
    [TotalNumberOfBuses]                          INT            NULL,
    [TotalNumberOfPowerUnits]                     INT            NULL,
    [FleetSizeCode]                               NVARCHAR (255) NULL,
    [NNDriversInterstateWithin100Miles]           NVARCHAR (255) NULL,
    [NNDriversInterstateBeyond100Miles]           NVARCHAR (255) NULL,
    [NNDriversInterstateTotal]                    INT            NULL,
    [NNDriversIntrastateWithin100Miles]           NVARCHAR (255) NULL,
    [NNDriversIntrastateBeyond100Miles]           NVARCHAR (255) NULL,
    [NNDriversIntrastateTotal]                    INT            NULL,
    [NNDriversAvgNumberTripLeasedDriversPerMonth] NVARCHAR (255) NULL,
    [NNDriversGrandTotalInterstateAndIntrastate]  INT            NULL,
    [NNDriversTotalWithCommercialDriversLicense]  NVARCHAR (255) NULL,
    [LatestReviewType]                            NVARCHAR (255) NULL,
    [LatestReviewDocumentNN]                      NVARCHAR (255) NULL,
    [LatestReviewDate]                            NVARCHAR (255) NULL,
    [RecordableAccidentRate]                      NVARCHAR (255) NULL,
    [PreventableRecordableAccidentRate]           NVARCHAR (255) NULL,
    [MileageCalendarYearMCS_150]                  INT            NULL,
    [ReviewMileageMCS_151]                        NVARCHAR (255) NULL,
    [SafetyRatingTypeCode]                        NVARCHAR (255) NULL,
    [SafetyRatingEffectiveDate]                   NVARCHAR (255) NULL,
    [MexicanNeighborhoodPhysical]                 NVARCHAR (255) NULL,
    [MexicanNeighborhoodMailing]                  NVARCHAR (255) NULL,
    [McsipStep]                                   NVARCHAR (255) NULL,
    [McsipDate]                                   NVARCHAR (255) NULL,
    [UserId]                                      NVARCHAR (255) NULL,
    [ReasonCodeAdd]                               INT            NULL,
    [ReasonCodeChange]                            NVARCHAR (255) NULL,
    [ReasonCodeDelete]                            NVARCHAR (255) NULL,
    [Mcs_150MileageYear]                          NVARCHAR (255) NULL,
    [DateAdded]                                   INT            NULL,
    [DateLastChanged]                             INT            NULL,
    [DateDeleted]                                 NVARCHAR (255) NULL,
    [TotalCars]                                   INT            NULL,
    [Version]                                     NVARCHAR (255) NULL,
    [CreationDate]                                INT            NULL,
    [AddUserId]                                   NVARCHAR (255) NULL,
    [DeleteUserId]                                NVARCHAR (255) NULL,
    [Mcs_150Date]                                 NVARCHAR (255) NULL,
    [RecordUpdatedFlag]                           INT            NULL,
    [EmailAddress]                                NVARCHAR (255) NULL,
    [UsdotRevokedFlag]                            NVARCHAR (255) NULL,
    [UsdotRevokedNumber]                          NVARCHAR (255) NULL,
    [CompanyRepresentativeOne]                    NVARCHAR (255) NULL,
    [CompanyRepresentativeTwo]                    NVARCHAR (255) NULL,
    [TrucksAndTractors]                           INT            NULL,
    [Latitude]                                    FLOAT (53)     NULL,
    [Longitude]                                   FLOAT (53)     NULL,
    [SortRelevance]                               INT            NULL,
    CONSTRAINT [PK_Main] PRIMARY KEY CLUSTERED ([USDOTNumber] ASC)
);

GO
CREATE NONCLUSTERED INDEX [IX_Address]
    ON [dbo].[Main]([PhysicalAddressNationality] ASC, [PhysicalAddressStateCode] ASC, [PhysicalAddressCity] ASC, [LegalName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ZIP]
    ON [dbo].[Main]([PhysicalAddressZipCode] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_MC]
    ON [dbo].[Main]([IccDocketNumberFirst] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TrucksAndTractors]
    ON [dbo].[Main]([TrucksAndTractors] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SortRelevance]
    ON [dbo].[Main]([SortRelevance] ASC);


GO
CREATE TABLE [dbo].[Main_CargoType] (
    [USDOTNumber] INT NOT NULL,
    [CargoNumber] INT NOT NULL
);


GO
CREATE TABLE [dbo].[Main_ServiceType] (
    [USDOTNumber]       INT NOT NULL,
    [ServiceTypeNumber] INT NOT NULL
);



GO
CREATE TABLE [dbo].[PreMain] (
    [Status]                                      NVARCHAR (255) NULL,
    [EntityType]                                  NVARCHAR (255) NULL,
    [USDOTNumber]                                 INT            NOT NULL,
    [LegalName]                                   NVARCHAR (255) NULL,
    [DoingBusinessAsName]                         NVARCHAR (255) NULL,
    [DunBradstreetNumber]                         NVARCHAR (255) NULL,
    [PhysicalAddressNationality]                  NVARCHAR (255) NULL,
    [PhysicalAddressFmcsaRegion]                  INT            NULL,
    [PhysicalAddressStreet]                       NVARCHAR (255) NULL,
    [PhysicalAddressCity]                         NVARCHAR (255) NULL,
    [PhysicalAddressCountyCode]                   INT            NULL,
    [PhysicalAddressStateCode]                    NVARCHAR (255) NULL,
    [PhysicalAddressZipCode]                      NVARCHAR (255) NULL,
    [UndeliverablePhysicalAddress]                NVARCHAR (255) NULL,
    [OfficeTelephoneNumber]                       NVARCHAR (255) NULL,
    [CellPhoneNumber]                             NVARCHAR (255) NULL,
    [OfficeFaxPhoneNumber]                        NVARCHAR (255) NULL,
    [MailingAddressNationality]                   NVARCHAR (255) NULL,
    [MailingAddressStreet]                        NVARCHAR (255) NULL,
    [MailingAddressCity]                          NVARCHAR (255) NULL,
    [MailingAddressCountyCode]                    INT            NULL,
    [MailingAddressStateCode]                     NVARCHAR (255) NULL,
    [MailingAddressZipCode]                       NVARCHAR (255) NULL,
    [UndeliverableMailingAddress]                 NVARCHAR (255) NULL,
    [OfficerInChargeCode]                         INT            NULL,
    [SafetyInvestigatorTerritoryCode]             NVARCHAR (255) NULL,
    [IccDocketNumber1Prefix]                      NVARCHAR (255) NULL,
    [IccDocketNumberFirst]                        NVARCHAR (255) NULL,
    [IccDocket2Prefix]                            NVARCHAR (255) NULL,
    [IccDocketNumberSecond]                       NVARCHAR (255) NULL,
    [IccDocket3Prefix]                            NVARCHAR (255) NULL,
    [IccDocketNumberThird]                        NVARCHAR (255) NULL,
    [Classification]                              NVARCHAR (255) NULL,
    [ClassificationOtherDefined]                  NVARCHAR (255) NULL,
    [OperationCarrierInterstate]                  NVARCHAR (255) NULL,
    [OperationCarrierIntrastateHazmat]            NVARCHAR (255) NULL,
    [OperationCarrierIntrastateNonHazmat]         NVARCHAR (255) NULL,
    [OperationShipperInterstate]                  NVARCHAR (255) NULL,
    [OperationShipperIntrastate]                  NVARCHAR (255) NULL,
    [OperationVehicleRegistrant]                  NVARCHAR (255) NULL,
    [BusinessOrganizationCode]                    NVARCHAR (255) NULL,
    [CargoTransportedAGeneralFreight]             NVARCHAR (255) NULL,
    [CargoTransportedBHouseholdGoods]             NVARCHAR (255) NULL,
    [CargoTransportedCMetalSheetsCoilsRolls]      NVARCHAR (255) NULL,
    [CargoTransportedDMotorVehicles]              NVARCHAR (255) NULL,
    [CargoTransportedEDriveawayTowaway]           NVARCHAR (255) NULL,
    [CargoTransportedFLogsPolesBeamsLumber]       NVARCHAR (255) NULL,
    [CargoTransportedGBuildingMaterials]          NVARCHAR (255) NULL,
    [CargoTransportedHMobileHomes]                NVARCHAR (255) NULL,
    [CargoTransportedIMachineryLargeObjects]      NVARCHAR (255) NULL,
    [CargoTransportedJFreshProduce]               NVARCHAR (255) NULL,
    [CargoTransportedKLiquidsGases]               NVARCHAR (255) NULL,
    [CargoTransportedLIintermodalContainers]      NVARCHAR (255) NULL,
    [CargoTransportedMPassengers]                 NVARCHAR (255) NULL,
    [CargoTransportedNOilfieldEquipment]          NVARCHAR (255) NULL,
    [CargoTransportedOLivestock]                  NVARCHAR (255) NULL,
    [CargoTransportedPGrainFeedHay]               NVARCHAR (255) NULL,
    [CargoTransportedQCoalCoke]                   NVARCHAR (255) NULL,
    [CargoTransportedRMeat]                       NVARCHAR (255) NULL,
    [CargoTransportedSGarbageRefuseTrash]         NVARCHAR (255) NULL,
    [CargoTransportedTUSMail]                     NVARCHAR (255) NULL,
    [CargoTransportedUChemicals]                  NVARCHAR (255) NULL,
    [CargoTransportedVCommoditiesDryBulk]         NVARCHAR (255) NULL,
    [CargoTransportedWRefrigeratedFood]           NVARCHAR (255) NULL,
    [CargoTransportedXBeverages]                  NVARCHAR (255) NULL,
    [CargoTransportedYPaperProducts]              NVARCHAR (255) NULL,
    [CargoTransportedZUtility]                    NVARCHAR (255) NULL,
    [CargoTransportedAAFarmSupplies]              NVARCHAR (255) NULL,
    [CargoTransportedBBConstruction]              NVARCHAR (255) NULL,
    [CargoTransportedCCWaterWell]                 NVARCHAR (255) NULL,
    [CargoTransportedDDOther]                     NVARCHAR (255) NULL,
    [CargoTransportedOtherDefined]                NVARCHAR (255) NULL,
    [HazmatIndicator]                             NVARCHAR (255) NULL,
    [NNEquipmentUnitsOwnedTruck]                  INT            NULL,
    [NNEquipmentUnitsOwnedTractor]                INT            NULL,
    [NNEquipmentUnitsOwnedTrailer]                INT            NULL,
    [NNEquipmentUnitsOwnedMotorCoach]             INT            NULL,
    [NNEquipmentUnitsOwnedSchoolBus1_8]           INT            NULL,
    [NNEquipmentUnitsOwnedSchoolBus9_15]          INT            NULL,
    [NNEquipmentUnitsOwnedSchoolBus16Plus]        INT            NULL,
    [NNEquipmentUnitsOwnedMiniBusVan16Plus]       INT            NULL,
    [NNEquipmentUnitsOwnedMiniBusVan1_8]          INT            NULL,
    [NNEquipmentUnitsOwnedMiniBusVan9_15]         INT            NULL,
    [NNEquipmentUnitsOwnedLimo1_8]                INT            NULL,
    [NNEquipmentUnitsOwnedLimo9_15]               INT            NULL,
    [NNEquipmentUnitsOwnedLimo16Plus]             INT            NULL,
    [NNEquipmentUnitsTermLeasedTruck]             INT            NULL,
    [NNEquipmentUnitsTermLeasedTractor]           INT            NULL,
    [NNEquipmentUnitsTermLeasedTrailer]           INT            NULL,
    [NNEquipmentUnitsTermLeasedMotorCoach]        INT            NULL,
    [NNEquipmentUnitsTermLeasedSchoolBus1_8]      INT            NULL,
    [NNEquipmentUnitsTermLeasedSchoolBus9_15]     INT            NULL,
    [NNEquipmentUnitsTermLeasedSchoolBus16Plus]   INT            NULL,
    [NNEquipmentUnitsTermLeasedMiniBusVan16Plus]  INT            NULL,
    [NNEquipmentUnitsTermLeasedMiniBusVan1_8]     INT            NULL,
    [NNEquipmentUnitsTermLeasedMiniBusVan9_15]    INT            NULL,
    [NNEquipmentUnitsTermLeasedLimo1_8]           INT            NULL,
    [NNEquipmentUnitsTermLeasedLimo9_15]          INT            NULL,
    [NNEquipmentUnitsTermLeasedLimo16Plus]        INT            NULL,
    [NNEquipmentUnitsTripLeasedTruck]             INT            NULL,
    [NNEquipmentUnitsTripLeasedTractor]           INT            NULL,
    [NNEquipmentUnitsTripLeasedTrailer]           INT            NULL,
    [NNEquipmentUnitsTripLeasedMotorCoach]        INT            NULL,
    [NNEquipmentUnitsTripLeasedSchoolBus1_8]      INT            NULL,
    [NNEquipmentUnitsTripLeasedSchoolBus9_15]     INT            NULL,
    [NNEquipmentUnitsTripLeasedSchoolBus16Plus]   INT            NULL,
    [NNEquipmentUnitsTripLeasedMiniBusVan16Plus]  INT            NULL,
    [NNEquipmentUnitsTripLeasedMiniBusVan1_8]     INT            NULL,
    [NNEquipmentUnitsTripLeasedMiniBusVan9_15]    INT            NULL,
    [NNEquipmentUnitsTripLeasedLimo1_8]           INT            NULL,
    [NNEquipmentUnitsTripLeasedLimo9_15]          INT            NULL,
    [NNEquipmentUnitsTripLeasedLimo16Plus]        INT            NULL,
    [TotalNumberOfTrucks]                         INT            NULL,
    [TotalNumberOfBuses]                          INT            NULL,
    [TotalNumberOfPowerUnits]                     INT            NULL,
    [FleetSizeCode]                               NVARCHAR (255) NULL,
    [NNDriversInterstateWithin100Miles]           NVARCHAR (255) NULL,
    [NNDriversInterstateBeyond100Miles]           NVARCHAR (255) NULL,
    [NNDriversInterstateTotal]                    INT            NULL,
    [NNDriversIntrastateWithin100Miles]           NVARCHAR (255) NULL,
    [NNDriversIntrastateBeyond100Miles]           NVARCHAR (255) NULL,
    [NNDriversIntrastateTotal]                    INT            NULL,
    [NNDriversAvgNumberTripLeasedDriversPerMonth] NVARCHAR (255) NULL,
    [NNDriversGrandTotalInterstateAndIntrastate]  NVARCHAR (255) NULL,
    [NNDriversTotalWithCommercialDriversLicense]  NVARCHAR (255) NULL,
    [LatestReviewType]                            NVARCHAR (255) NULL,
    [LatestReviewDocumentNN]                      NVARCHAR (255) NULL,
    [LatestReviewDate]                            NVARCHAR (255) NULL,
    [RecordableAccidentRate]                      NVARCHAR (255) NULL,
    [PreventableRecordableAccidentRate]           NVARCHAR (255) NULL,
    [MileageCalendarYearMCS_150]                  INT            NULL,
    [ReviewMileageMCS_151]                        NVARCHAR (255) NULL,
    [SafetyRatingTypeCode]                        NVARCHAR (255) NULL,
    [SafetyRatingEffectiveDate]                   NVARCHAR (255) NULL,
    [MexicanNeighborhoodPhysical]                 NVARCHAR (255) NULL,
    [MexicanNeighborhoodMailing]                  NVARCHAR (255) NULL,
    [McsipStep]                                   NVARCHAR (255) NULL,
    [McsipDate]                                   NVARCHAR (255) NULL,
    [UserId]                                      NVARCHAR (255) NULL,
    [ReasonCodeAdd]                               INT            NULL,
    [ReasonCodeChange]                            NVARCHAR (255) NULL,
    [ReasonCodeDelete]                            NVARCHAR (255) NULL,
    [Mcs_150MileageYear]                          NVARCHAR (255) NULL,
    [DateAdded]                                   INT            NULL,
    [DateLastChanged]                             INT            NULL,
    [DateDeleted]                                 NVARCHAR (255) NULL,
    [TotalCars]                                   INT            NULL,
    [Version]                                     NVARCHAR (255) NULL,
    [CreationDate]                                INT            NULL,
    [AddUserId]                                   NVARCHAR (255) NULL,
    [DeleteUserId]                                NVARCHAR (255) NULL,
    [Mcs_150Date]                                 NVARCHAR (255) NULL,
    [RecordUpdatedFlag]                           INT            NULL,
    [EmailAddress]                                NVARCHAR (255) NULL,
    [UsdotRevokedFlag]                            NVARCHAR (255) NULL,
    [UsdotRevokedNumber]                          NVARCHAR (255) NULL,
    [CompanyRepresentativeOne]                    NVARCHAR (255) NULL,
    [CompanyRepresentativeTwo]                    NVARCHAR (255) NULL
);



GO
CREATE TABLE [dbo].[ServiceType] (
    [ServiceTypeNumber] INT            IDENTITY (1, 1) NOT NULL,
    [Service Type]      NVARCHAR (MAX) NOT NULL,
    CONSTRAINT [PK_ServiceType] PRIMARY KEY CLUSTERED ([ServiceTypeNumber] ASC)
);


GO
CREATE TABLE [dbo].[States] (
    [CountryCode] NVARCHAR (5)  NOT NULL,
    [State]       NVARCHAR (50) NOT NULL,
    [StateCode]   NVARCHAR (5)  NOT NULL
);




GO
ALTER TABLE [dbo].[DeletedRecords]
    ADD CONSTRAINT [DF_DeletedRecords_DeletedOn] DEFAULT (getdate()) FOR [DeletedOn];



GO
ALTER TABLE [dbo].[Main_CargoType] WITH NOCHECK
    ADD CONSTRAINT [FK_Main_CargoType_CargoType] FOREIGN KEY ([CargoNumber]) REFERENCES [dbo].[CargoType] ([CargoNumber]) ON DELETE CASCADE;



GO
ALTER TABLE [dbo].[Main_CargoType] WITH NOCHECK
    ADD CONSTRAINT [FK_Main_CargoType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[Main] ([USDOTNumber]) ON DELETE CASCADE;




GO
ALTER TABLE [dbo].[Main_ServiceType] WITH NOCHECK
    ADD CONSTRAINT [FK_Main_ServiceType_Main] FOREIGN KEY ([USDOTNumber]) REFERENCES [dbo].[Main] ([USDOTNumber]) ON DELETE CASCADE;





GO
ALTER TABLE [dbo].[Main_ServiceType] WITH NOCHECK
    ADD CONSTRAINT [FK_Main_ServiceType_ServiceType] FOREIGN KEY ([ServiceTypeNumber]) REFERENCES [dbo].[ServiceType] ([ServiceTypeNumber]) ON DELETE CASCADE;




GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1 - By Owner, 
2 - By Admin', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DeletedRecords', @level2type = N'COLUMN', @level2name = N'RecordDeletedBy';



GO
ALTER TABLE [dbo].[Main_CargoType] WITH CHECK CHECK CONSTRAINT [FK_Main_CargoType_CargoType];

ALTER TABLE [dbo].[Main_CargoType] WITH CHECK CHECK CONSTRAINT [FK_Main_CargoType_Main];

ALTER TABLE [dbo].[Main_ServiceType] WITH CHECK CHECK CONSTRAINT [FK_Main_ServiceType_Main];

ALTER TABLE [dbo].[Main_ServiceType] WITH CHECK CHECK CONSTRAINT [FK_Main_ServiceType_ServiceType];


GO
PRINT N'Update complete.';


GO
