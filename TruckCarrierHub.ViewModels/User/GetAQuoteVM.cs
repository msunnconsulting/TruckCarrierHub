using Common.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PartnerCarrier.ViewModels.User
{
    public class GetAQuoteVM
    {
        [Required(ErrorMessage = "Pickup Location field is required")]
        public string PickupLocation { get; set; }
        [Required(ErrorMessage = "Delivery Location field is required")]
        public string DeliveryLocation { get; set; }

        public string LoadType { get; set; }

        public string SpecialHandling { get; set; }

        public List<LoadTypeVM> ListLoad { get; set; }

        public List<LocationTypeVM> ListPickupLocationType { get; set; }

        public List<LocationTypeVM> ListDeliveryLocationType { get; set; }
        public string PickupLocationType { get; set; }
        public string DeliveryLocationType { get; set; }

        [Required(ErrorMessage = "First Name field is required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name field is required")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email Address field is required")]
        [RegularExpression(Patterns.EmailValidation, ErrorMessage = "Please enter valid email address")]
        [MaxLength(255, ErrorMessage = "{0} cannot be longer than 255 characters.")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Phone field is required")]
        public string Phone { get; set; }

        public string CompanyName { get; set; }

        public List<SpecialHandlingVM> PickupSpecialHandlingList { get; set; }

        public List<SpecialHandlingVM> DeliverySpecialHandlingList { get; set; }

        public List<int> SelectedSpcialHandlings { get; set; }

        public string DescriptionOfGoods { get; set; }

        public string NumberOfItem { get; set; }

        [Required(ErrorMessage = "Pickup Date field is required")]
        [DisplayFormat(DataFormatString = "{0:MM'/'dd'/'yyyy}")]
        [DataType(DataType.Date)]
        public DateTime PickupDate { get; set; }
        public string StringPickupDate { get; set; }

        public List<LoadInformationVM> ListOfLoadInformationVM { get; set; }

        public List<LoadClassVM> LoadClassList { get; set; }
        public int? LoadClassId { get; set; }

        public List<LoadItemTypeVM> LoadItemTypeList { get; set; }
        public string LoadItemTypeId { get; set; }

        public string selectedSpecialHandlingIds { get; set; }
        public string selectedDeliverySpecialHandlingIds { get; set; }
        public string selectedSpecialHandlingValues { get; set; }

        public string selectedDeliverySpecialHandlingValue { get; set; }

        public bool IsFlexible { get; set; }

        public List<LoadContainerTypeVM> LoadContainerTypeList { get; set; }

        public int? LoadStatusTypeId { get; set; }

        public List<LoadContainerLengthVM> LoadContainerLengthList { get; set; }

        public int LoadContainerLengthId { get; set; }
        public string OriginURL { get; set; }

        public List<TemperatureVM> TemperatureList { get; set; }
        public int TemperatureId { get; set; }
        public string TemperatureType { get; set; }

        public List<RefrigerationVM> RefrigerationList { get; set; }
        public int? RefrigerationId { get; set; }
        public string RefrigerationType { get; set; }
        public string Temperature { get; set; }

        public List<LoadTruckTypeVM> TruckTypeList { get; set; }
        public int TruckTypeId { get; set; }

        public List<LoadInfoVM> LoadInfoList { get; set; }
        public int LoadInfoId { get; set; }

        public LoadInformationVM LoadInformationVM { get; set; }

        public string LoadDetailsDescription { get; set; }

        public string PickupLocationTypeValue { get; set; }

        public string DeliveryLocationTypeValue { get; set; }
        public int QuoteId { get; set; }

    }
    public class TemperatureVM
    {
        public int Id { get; set; }
        public string TemperatureType { get; set; }
    }
    public class LoadInfoVM
    {
        public int Id { get; set; }
        public string LoadInfoType { get; set; }
    }

    public class LoadTruckTypeVM
    {
        public int Id { get; set; }
        public string TruckType { get; set; }
    }

    public class RefrigerationVM
    {
        public int Id { get; set; }
        public string RefrigerationType { get; set; }
    }

    public class LoadContainerTypeVM
    {
        public int Id { get; set; }
        public string StatusType { get; set; }
    }

    public class LoadContainerLengthVM
    {
        public int Id { get; set; }
        public string LengthOfContainer { get; set; }
    }


    public class LoadClassVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class LoadInformationVM
    {
        [Required(ErrorMessage = "This Goods Description field is required")]
        public string GoodDescription { get; set; }

        public int? NumberOfItem { get; set; }
        public int? LoadStatusTypeId { get; set; }
        public int? LoadItemTypeId { get; set; }
        [Required(ErrorMessage = "Length field is required")]
        public int? DimentionLength { get; set; }
        [Required(ErrorMessage = "Width field is required")]
        public int? DimentionWidth { get; set; }
        [Required(ErrorMessage = "Height field is required")]
        public int? DimentionHeight { get; set; }

        [Required(ErrorMessage = "This Weight field is required")]
        public int WeightPerItem { get; set; }
        public int? ClassTypeId { get; set; }
        public string ClassType { get; set; }
        public int? LoadInfoId { get; set; }
        public bool IsHazmat { get; set; }
        public bool IsNonStackable { get; set; }
        public int? TruckTypeId { get; set; }
        public int? LoadContainerLengthId { get; set; }
        public int? NoOfContainers { get; set; }

        public string TruckType { get; set; }
        public string LoadInfo { get; set; }
        public string LoadStatusType { get; set; }
        public string LoadItemType { get; set; }
        public string LoadContainerLength { get; set; }

    }

    public class LoadItemTypeVM
    {
        public int Id { get; set; }
        public string LoadItemType { get; set; }
    }

    public class SpecialHandlingVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int? LocationTypeId { get; set; }
    }

    public class LocationTypeVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
    }

    public class LoadTypeVM
    {
        public int Id { get; set; }
        public string LoadName { get; set; }
        public string LoadDescription { get; set; }

        public bool IsSelected { get; set; }

    }
}
