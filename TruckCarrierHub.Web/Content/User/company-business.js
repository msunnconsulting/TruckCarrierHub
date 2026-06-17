(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.SaveCompanyBusinessOnSuccess = SaveCompanyBusinessOnSuccess;
    vm.SaveCompanyBusinessOnFailure = SaveCompanyBusinessOnFailure;
    vm.SubmitBusinessCompany = SubmitBusinessCompany;
    var FinaltrailerTypeSelectedItems = "";
    var FinaldriverTypeSelectedItems = "";


    function SubmitBusinessCompany() {
        var trailerTypeSelectedItems = "";
        //get entitytype checkbox values
        $("[name='SelectedTrailerTypes']input:checked").each(function () {
            var item = $(this);

            if (trailerTypeSelectedItems == "") {
                trailerTypeSelectedItems = $(this).val(); +",";
            }
            else { trailerTypeSelectedItems += "," + $(this).val(); }
        });

        var driverTypeSelectedItems = "";
        //get entitytype checkbox values
        $("[name='SelectedDriverTypes']input:checked").each(function () {
            var item = $(this);
           
            if (driverTypeSelectedItems == "") {
                driverTypeSelectedItems = $(this).val(); +",";
            }
            else { driverTypeSelectedItems += "," + $(this).val(); }
        });

        if (trailerTypeSelectedItems == "" && driverTypeSelectedItems == "") {
            $("#IsNowHiring").val(false)
        }
        else {
            $("#IsNowHiring").val(true)
          
        }
        $("#frmCompanyBusiness").submit();

    }

    $(document).ready(function () {
        //Get parameter value using query string
        var Update = getUrlParameter('update');
         
        //if parameter is update then display update message
        if (Update != undefined && Update != '' && Update == "true") {
            showAlertMessage("#dvNotification", "success", "Business Password Changed Successfully");
        }
    });

    function SaveCompanyBusinessOnSuccess(response) {
        if (response == "") {
            window.location.href = "/business-saved-successfully";
        }
        else {
            showAlertMessage("#dvNotification", "success", "Business Updated Successfully");
        }
        
    }
    function SaveCompanyBusinessOnFailure(XMLHttpRequest, textStatus, erroThrown) {
    }

    $('.trailer-dropdown').on('hide.bs.dropdown', function ApplyCargoTypeFilters() {

        $(".filterApplyBtnForCargoType").hide();
        var trailerTypeSelectedItems = "";
        //get entitytype checkbox values
        $("[name='SelectedTrailerTypes']input:checked").each(function () {
            var item = $(this);
            if (trailerTypeSelectedItems == "") {
                trailerTypeSelectedItems = $(this).val(); +",";
            }
            else {
                trailerTypeSelectedItems += "," + $(this).val();
            }
        });
        FinaltrailerTypeSelectedItems = trailerTypeSelectedItems;
        if (FinaltrailerTypeSelectedItems == "" && FinaldriverTypeSelectedItems == "") {
            $("#IsNowHiring").val(false)
        }
        else {
            $("#IsNowHiring").val(true)
           
        }
    });

    $('.driver-dropdown').on('hide.bs.dropdown', function ApplyCargoTypeFilters() {

        $(".filterApplyBtnForCargoType").hide();
        var driverTypeSelectedItems = "";
        //get entitytype checkbox values
        $("[name='SelectedDriverTypes']input:checked").each(function () {
            var item = $(this);
            if (driverTypeSelectedItems == "") {
                driverTypeSelectedItems = $(this).val(); +",";
            }
            else { driverTypeSelectedItems += "," + $(this).val(); }
        });

        FinaldriverTypeSelectedItems = driverTypeSelectedItems;

        if (FinaltrailerTypeSelectedItems == "" && FinaldriverTypeSelectedItems == "") {
           
            $("#IsNowHiring").val(false)
        }
        else {
            $("#IsNowHiring").val(true)
        }

    });

})(this);