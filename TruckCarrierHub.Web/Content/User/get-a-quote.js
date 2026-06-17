(function (global) {
    "use strict";

    //variables
    var vmGetAQuote = global.vmGetAQuote = {};

    //function
    vmGetAQuote.ChangeLoadType = ChangeLoadType;
    vmGetAQuote.ChangeLocationType = ChangeLocationType;

    vmGetAQuote.OnSuccessSubmitGetAQuote = OnSuccessSubmitGetAQuote;
    vmGetAQuote.OnFailureSubmitGetAQuote = OnFailureSubmitGetAQuote;
    vmGetAQuote.calculateTotalWeight = calculateTotalWeight;
    vmGetAQuote.CheckForValidation = CheckForValidation;
    vmGetAQuote.CheckForNoOfItem = CheckForNoOfItem;
    vmGetAQuote.CheckForLWH = CheckForLWH;
    vmGetAQuote.CheckValidationForWeight = CheckValidationForWeight;
    vmGetAQuote.SubmitQuote = SubmitQuote;
 
    var TotalWeightLabelToShow = 0;

    //page initialize
    $(document).ready(function () {

        //Initialization of popover
        InitializePopover()

        //Initialize date picker control
        $('.datepicker').datepicker({
            changeMonth: true,
            changeYear: true,
            format: 'MM/dd/yyyy',
            autoclose: true,
            onClose: function (dateText, inst) {
                $(this).focus();
            }
        }).val();

        //Reverse pickup and delivery locations
        //Swaping or Interchanging Pickup Location value with Delivery Location
        $('#swapePickupLocation').on("click",function () {
            var deliveryValue = $("#DeliveryLocation").val();
            var pickupValue = $("#PickupLocation").val();

            $("#PickupLocation").val(deliveryValue);
            $("#DeliveryLocation").val(pickupValue);
        });

        $('#swapeDeliveryLocation').on('click',function () {
            var deliveryValue = $("#DeliveryLocation").val();
            var pickupValue = $("#PickupLocation").val();
            $("#PickupLocation").val(deliveryValue);
            $("#DeliveryLocation").val(pickupValue);
        });

    });

    function InitializePopover() {
       
        $('[data-toggle="popover"]').popover({
            placement: 'top',
        });
    }

    //Calculate Total weight to show at the bottom for quote total weight
    function calculateTotalWeight(test) {

        var noOfItem = 0;
        var weightPerItem = 0;
        var total = 0;

        $("#LoadrowforLTL input").each(function () {
            if (this.className == "form-control multiNoOfItem valid") {
                noOfItem = Number(this.value == "" ? 0 : this.value)
            }
            if (this.className == "form-control multiweight valid") {
                weightPerItem = Number(this.value == "" ? 0 : this.value)
            }

            if (noOfItem > 0 && weightPerItem > 0) {
                total += noOfItem * weightPerItem;
                noOfItem = 0;
                weightPerItem = 0;
            }
            $('.total_weight_show').text(total);
        });
    }

    $('.noOfItem').on('focusout', function () {
        var weight = Number($(".weight").val());
        var noOFItem = Number($(this).val());
        TotalWeightLabelToShow = weight * noOFItem;
        $('.total_weight_show').text(TotalWeightLabelToShow);
        $('.total_container_show').text(noOFItem);
    });

    $('.weight').on('focusout', function () {
        var noOFItem = Number($(".noOfItem").val() == undefined ? $(".multiNoOfItem ").val() : $(".noOfItem").val());
        var weight = Number($(this).val());

        TotalWeightLabelToShow = weight * noOFItem;
        $('.total_weight_show').text(TotalWeightLabelToShow);
        $('.total_container_show').text(noOFItem);
    });

    $('.onlyTotalWeight').on('focusout', function () {
        $('.total_weight_show').text($(".onlyTotalWeight").val() == "" ? 0 : $(".onlyTotalWeight").val());
    });

    $('#RefrigerationId').on('change', function () {

        if ($('#RefrigerationId :selected').text() == "Exact temperature") {
            $('.temperature').find('input, select').removeAttr('disabled');
        }
        else {
            $('.temperature').find('input, select').attr('disabled', 'disable');
        }
    });

    //on change dropdown of Load type
    function ChangeLoadType() {
        window.location.href = "/getaquote?LoadType=" + $("#LoadType").val();
    }

    //on change dropdown of Location Type
    function ChangeLocationType(locationType) {
        var url = "/GetCheckboxListFromLocationType";
        var loadType = $("#LoadType").val();
        if (locationType == "Pickup") {
            var locationTypeId = Number($("#PickupLocationType").val());
            ShowSpinner();
            $.ajax({
                url: url,
                data: {
                    locationTypeId: locationTypeId,
                    locationType: locationType,
                    loadType: loadType
                },
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    HideSpinner();
                    //Update UI content you want to update at client side based on bakend response
                    //before update new content first delete the old content
                    $(".pickupCheckboxes").remove();

                    var i;
                    for (i = 0; i < response.length; i++) {
                        var stringtoAppend = '<div class="checkbox checkbox-primary pickupCheckboxes"><input id="' + response[i].Id + '" name="SelectedSpcialHandlings" type="checkbox" value="' + response[i].Name + '" title="' + response[i].Title + '"> <label class="check" for="' + response[i].Id + '" data-toggle="popover" data-trigger="hover" data-placement="left" data-content="' + response[i].Title + '"> '+response[i].Name+'</label></div>'
                        $(".pickup-special-handling").append(stringtoAppend);
                    }
                    //Initialization of popover
                    InitializePopover();
                },
                error: function (response) {
                },
                failure: function (response) {
                }
            });
        }
        else {
            var deliveryLocationId = $("#DeliveryLocationType").val();
            ShowSpinner();
            $.ajax({
                url: url,
                data: {
                    locationTypeId: deliveryLocationId,
                    locationType: locationType,
                    loadType: loadType
                },
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: (response) => {
                    HideSpinner();
                    //Update UI content you want to update at client side based on bakend response
                    //before update new content first delete the old content
                    $(".deliveryCheckboxes").remove();
                    var i;
                    for (i = 0; i < response.length; i++) {
                        var stringtoAppend = '<div class="checkbox checkbox-primary deliveryCheckboxes"><input id="' + response[i].Id + '" name="SelectedDeliverySpecialHandlings" type="checkbox" value="' + response[i].Name + '"> <label class="check" for="' + response[i].Id + '"  data-toggle="popover" data-trigger="hover" data-placement="left" data-content="' + response[i].Title + '">' + response[i].Name + '</label></div>'
                        $(".delivery-special-handling").append(stringtoAppend);
                    }
                    //Initialization of popover
                    InitializePopover();
                },
                error: function (response) {
                },
                failure: (response) => {
                }
            });
        }
    }

    function OnSuccessSubmitGetAQuote() {
         
    }

    function OnFailureSubmitGetAQuote() {
    }

    //Remove section on close button cliked on corner of the panel section
    $(document).off().on('click', "a.removeSection", function () {
        $(this).parent().remove();
        calculateTotalWeight();
    });

    //Add Load More function which copy the exact Load Information div 
    $("#add_load").unbind("click").click(function () {
       
        //First copy the div 
        var clonedDIV = $('#appendAddLoadHere').clone();
        //after copy make input fields blank
        clonedDIV.find('input').val('');

        //remove style from the div
        clonedDIV[0].children[0].removeAttribute("style")

        //Append the copied div to specifield div where we want to append
        $('#LoadrowforLTL').append(clonedDIV);

    });

    function CheckForValidation(e) {
        if ($(e).children().next().val() == "" || $(e).children().next().val() == undefined) {
            $(e).children().next().next().css("display", "block");
        }
        else {
            $(e).children().next().next().css("display", "none");
        }
    }

    function CheckForNoOfItem(e) {
        if ($(e).children().children().next().val() == "" || $(e).children().children().next().val() == undefined) {
            $(e).next().css("display", "block");
        }
        else {
            $(e).next().css("display", "none");
        }
    }

    function CheckForLWH(e) {
        if ($(e).children().children().next().val() == "" || $(e).children().next().children().next().val() == "" || $(e).children().next().next().children().next().children().val() == "") {
            $(e).next().css("display", "block");
        }
        else {
            $(e).next().css("display", "none");
        }
    }

    function CheckValidationForWeight(e) {
        if ($(e).children().val() == "" || $(e).children().val() == undefined) {
            $(e).next().css("display", "block");
        }
        else {
            $(e).next().css("display", "none");
        }
    }

    //Submit Quote Detail by GetAQuoteVM view modal uisng ajax request
    function SubmitQuote() {
        //Create array to save list of Load information
        var ListOfloadInformationVMs = new Array();

        //get Load Type
        var loadtype = $("#LoadType").val().trim();

        //Getting Checkbox selected ids for Pickup Special Handlings
        var selectedSpecialHandlingIds = [];
        var selectedSpecialHandlingValues = [];
        $.each($("input[name='SelectedSpcialHandlings']:checked"), function () {
            selectedSpecialHandlingIds.push($(this).attr("id"));
            selectedSpecialHandlingValues.push($(this).attr("value"))
        });


        //Getting Checkbox selected ids for Delivery Special Hadnlings
        var selectedDeliverySpecialHandlingIds = [];
        var selectedDeliverySpecialHandlingValues = [];
        $.each($("input[name='SelectedDeliverySpecialHandlings']:checked"), function () {
            selectedDeliverySpecialHandlingIds.push($(this).attr("id"));
            selectedDeliverySpecialHandlingValues.push($(this).attr("value"))
        });

        var GetAQuoteVM = {};
        if (loadtype == "LTL") {

            //Find every row by iterating id inside class to get list of load information from DOM
            $("#LoadrowforLTL").find('.loafInformation').each(function () {

                //Create empty list to add list of code type
                var listofColumnsForLoadInformation = {};
                //Get load Information one by one for every list                
                //Set values into array with key values for send to controller as list
                listofColumnsForLoadInformation.GoodDescription = $(this).find("[data-loadinformation='LoadInformation']").val();
                listofColumnsForLoadInformation.NumberOfItem = $(this).find("[data-numberofitem='NumberOfItem']").val();
                listofColumnsForLoadInformation.LoadItemTypeId = $(this).find("[data-loaditemtypeid='LoadItemTypeId']").val();

                //listofColumnsForLoadInformation.LoadItemType = $(this).find("[data-loaditemtypeid='LoadItemTypeId']").val()
                listofColumnsForLoadInformation.LoadItemType = $(this).find("[data-loaditemtypeid='LoadItemTypeId'] :selected").text();

                listofColumnsForLoadInformation.DimentionLength = $(this).find("[data-DimentionLength='DimentionLength']").val();
                listofColumnsForLoadInformation.DimentionWidth = $(this).find("[data-DimentionWidth='DimentionWidth']").val();
                listofColumnsForLoadInformation.DimentionHeight = $(this).find("[data-DimentionHeight='DimentionHeight']").val();
                listofColumnsForLoadInformation.WeightPerItem = $(this).find("[data-WeightPerItem='WeightPerItem']").val();
                listofColumnsForLoadInformation.ClassTypeId = $(this).find("[data-ClassTypeId='ClassTypeId']").val();
                listofColumnsForLoadInformation.ClassType = $(this).find("[data-ClassTypeId='ClassTypeId'] :selected").text();

                listofColumnsForLoadInformation.IsHazmat = $(this).find("[data-IsHazmat='IsHazmat']").prop('checked');
                listofColumnsForLoadInformation.IsNonStackable = $(this).find("[data-IsNonStackable='IsNonStackable']").prop('checked');

                //Push all list one by one with values
                ListOfloadInformationVMs.push(listofColumnsForLoadInformation);

            });

            //Create variable to get a quote and  list of load information and get it into controller
            GetAQuoteVM = {
                FirstName: $("#FirstName").val(),
                LastName: $("#LastName").val(),
                EmailAddress: $("#EmailAddress").val(),
                Phone: $("#Phone").val(),
                CompanyName: $("#CompanyName").val(),
                OriginURL: $("#OriginURL").val(),
                //Get Quote Information 
                PickupLocation: $("#PickupLocation").val(),
                PickupLocationType: $("#PickupLocationType").val(),
                PickupLocationTypeValue: $("#PickupLocationType option:selected").text(),
                DeliveryLocation: $("#DeliveryLocation").val(),
                DeliveryLocationType: $("#DeliveryLocationType").val(),
                DeliveryLocationTypeValue: $("#DeliveryLocationType option:selected").text(),
                LoadType: loadtype,
                StringPickupDate: $("#PickupDate").val(),
                IsFlexible: $("#isFlexible").prop('checked'),
                selectedDeliverySpecialHandlingIds: selectedDeliverySpecialHandlingIds.join(","),
                selectedSpecialHandlingValues: selectedSpecialHandlingValues.join(", "),
                selectedSpecialHandlingIds: selectedSpecialHandlingIds.join(","),
                selectedDeliverySpecialHandlingValue: selectedDeliverySpecialHandlingValues.join(","),
                "ListOfLoadInformationVM": ListOfloadInformationVMs,
                LoadDetailsDescription: $("#SpecialInstructions").val(),
                RefrigerationId: $("#loadInfoTable1").find("[data-refrigerationid='RefrigerationId']").val(),
                TemperatureId: $("#loadInfoTable1").find("[data-temperatureid='TemperatureId']").val(),
                RefrigerationType: $("#loadInfoTable1").find("[data-refrigerationid='RefrigerationId'] :selected").text(),
                TemperatureType: $("#loadInfoTable1").find("[data-temperatureid='TemperatureId'] :selected").text(),
                Temperature: $("#loadInfoTable1").find("[data-refrigeration-temperature='Temperature']").val(),
            }
        }
        if(loadtype != "LTL") {
            var LoadInformationVM = {};
            
            if (loadtype == "FTL/Rail") {
                LoadInformationVM = {
                    GoodDescription: $("#loadInfoTable").find("[data-gooddescription='GoodDescription']").val(),
                    NumberOfItem: $("#loadInfoTable").find("[data-noofitems='NumberOfItem']").val(),
                    WeightPerItem: $("#loadInfoTable").find("[data-weightperitem='WeightPerItem']").val(),

                    TruckTypeId: $("#loadInfoTable").find("[data-trucktypeid='TruckTypeId']").val(),
                    TruckType: $("#loadInfoTable").find("[data-trucktypeid='TruckTypeId'] :selected").text(),
                    LoadInfoId: $("#loadInfoTable").find("[data-loadinfoid='LoadInfoId']").val(),
                    LoadInfo: $("#loadInfoTable").find("[data-loadinfoid='LoadInfoId'] :selected").text(),
                    LoadItemTypeId: $("#loadInfoTable").find("[data-loaditemtypeid='LoadItemTypeId']").val(),
                    LoadItemType: $("#loadInfoTable").find("[data-loaditemtypeid='LoadItemTypeId'] :selected").text(),
                    IsHazmat: $("#loadInfoTable").find("#isHazmat").prop('checked'),
                }

            }
            if (loadtype == "Flatbed") {

                LoadInformationVM = {
                    GoodDescription: $("#loadInfoTable").find("[data-gooddescription='GoodDescription']").val(),
                    DimentionLength: $("#loadInfoTable").find("[data-dimentionlength='DimentionLength']").val(),
                    DimentionWidth: $("#loadInfoTable").find("[data-dimentionwidth='DimentionWidth']").val(),
                    DimentionHeight: $("#loadInfoTable").find("[data-dimentionheight='DimentionHeight']").val(),
                    WeightPerItem: $("#loadInfoTable").find("[data-weightperitem='WeightPerItem']").val(),
                    TruckTypeId: $("#loadInfoTable").find("[data-trucktypeid='TruckTypeId']").val(),
                    TruckType: $("#loadInfoTable").find("[data-trucktypeid='TruckTypeId'] :selected").text(),
                    IsHazmat: $("#loadInfoTable").find("#isHazmat").prop('checked'),
                    IsNonStackable: $("#loadInfoTable").find("#isNonStackable").prop('checked')
                }
            }

            if (loadtype == "Container") {
                LoadInformationVM = {
                    GoodDescription: $("#loadInfoTable").find("[data-gooddescription='GoodDescription']").val(),
                    LoadStatusType: $("#loadInfoTable").find("[data-loadstatustypeid='LoadStatusTypeId'] :selected").text(),
                    LoadStatusTypeId: $("#loadInfoTable").find("[data-loadstatustypeid='LoadStatusTypeId']").val(),

                    NoOfContainers: $("#loadInfoTable").find("[data-noofcontainers='NoOfContainers']").val(),
                    LoadContainerLength: $("#loadInfoTable").find("[data-loadcontainerlengthid='LoadContainerLengthId'] :selected").text(),
                    LoadContainerLengthId: $("#loadInfoTable").find("[data-loadcontainerlengthid='LoadContainerLengthId']").val(),

                    WeightPerItem: $("#loadInfoTable").find("[data-weightperitem='WeightPerItem']").val(),
                    IsHazmat: $("#loadInfoTable").find("#isHazmat").prop('checked'),
                }
            }


            //Create variable to get list of suspect information and crime information and get it into controller
            GetAQuoteVM = {
                FirstName: $("#FirstName").val(),
                LastName: $("#LastName").val(),
                EmailAddress: $("#EmailAddress").val(),
                Phone: $("#Phone").val(),
                CompanyName: $("#CompanyName").val(),
                OriginURL: $("#OriginURL").val(),
                //Get Quote Information 
                PickupLocation: $("#PickupLocation").val(),
                PickupLocationType: $("#PickupLocationType").val(),
                PickupLocationTypeValue: $("#PickupLocationType option:selected").text(),
                DeliveryLocation: $("#DeliveryLocation").val(),
                DeliveryLocationType: $("#DeliveryLocationType").val(),
                DeliveryLocationTypeValue: $("#DeliveryLocationType option:selected").text(),
                LoadType: loadtype,
                StringPickupDate: $("#PickupDate").val(),
                IsFlexible: $("#isFlexible").prop('checked'),
                selectedDeliverySpecialHandlingIds: selectedDeliverySpecialHandlingIds.join(","),
                selectedSpecialHandlingValues: selectedSpecialHandlingValues.join(", "),
                selectedSpecialHandlingIds: selectedSpecialHandlingIds.join(","),
                selectedDeliverySpecialHandlingValue: selectedDeliverySpecialHandlingValues.join(","),
                "LoadInformationVM": LoadInformationVM,
                LoadDetailsDescription: $("#SpecialInstructions").val(),
                RefrigerationId: $("#loadInfoTable1").find("[data-refrigerationid='RefrigerationId']").val(),
                TemperatureId: $("#loadInfoTable1").find("[data-temperatureid='TemperatureId']").val(),
                RefrigerationType: $("#loadInfoTable1").find("[data-refrigerationid='RefrigerationId'] :selected").text(),
                TemperatureType: $("#loadInfoTable1").find("[data-temperatureid='TemperatureId'] :selected").text(),
                Temperature: $("#loadInfoTable1").find("[data-refrigeration-temperature='Temperature']").val(),
            }
        }
        var url = "/SubmitGetAQuotePage";
        var validForm = true;
        if (loadtype == "LTL") {
            $("#LoadrowforLTL input[type=text]").each(function () {
                if (this.value == "") {
                    $(this).next().css("display", "block");
                    validForm = false;
                }
            });
            $("#LoadrowforLTL input[type=number]").each(function () {
                if (this.value == "") {
                    $(this).parent().next().parent().next().css("display", "block");
                    $(this).next().parent().next().css("display", "block");
                    validForm = false;
                }
            });
        }
        if (loadtype != "LTL" && loadtype != "Flatbed") {
            $("#loadContainerID input[type=text]").each(function () {
                if (this.value == "") {
                    $(this).next().css("display", "block");
                    validForm = false;
                }
            });

            $("#loadContainerID input[type=number]").each(function () {
                if (this.value == "") {
                    $(this).parent().next().parent().next().css("display", "block");
                    $(this).next().parent().next().css("display", "block");
                    validForm = false;
                }
            });
        }

        if (loadtype == "Flatbed") {
            $("#loadContainerID input[type=text]").each(function () {
                if (this.value == "") {
                    $(this).next().css("display", "block");
                    validForm = false;
                }
            });

            $("#loadContainerID input[type=number]").each(function () {
                if (this.value == "") {
                    $(this).parent().next().parent().next().css("display", "block");
                    $(this).next().parent().next().css("display", "block");
                    validForm = false;
                }
            });
        }
        
        var isValid = $("#submitGetAQuoteForm").valid();
        if (validForm && isValid) {
            ShowSpinner();
            
            $.ajax({
                type: "Post",
                url: url,
                data: JSON.stringify({ getAQuoteVM: GetAQuoteVM }),
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    HideSpinner();
                    window.location.href = "/quote-submited-sucessfully"
                },
                error: function (response) {
                },
                failure: function (response) {
                }
            });
        }
    }

})(this);