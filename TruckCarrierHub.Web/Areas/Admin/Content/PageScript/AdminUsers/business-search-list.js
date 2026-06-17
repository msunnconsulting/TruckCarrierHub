(function (global) {

    var vm = global.vm = {};
    vm.pageSort = pageSort;
    vm.OnSuccessBindBusinessSearchList = OnSuccessBindBusinessSearchList;
    vm.OnFailureBusinessSearchList = OnFailureBusinessSearchList;
    vm.DeleteBusiness = DeleteBusiness;
    vm.OnSuccessDeleteBusinessById = OnSuccessDeleteBusinessById;
    vm.OnFailureDeleteBusinessById = OnFailureDeleteBusinessById;

    vm.EditBusinessOnSuccess = EditBusinessOnSuccess;
    vm.EditBusinessOnFailure = EditBusinessOnFailure;

    vm.OnSuccessResetPasswordBusiness = OnSuccessResetPasswordBusiness;
    vm.OnFailureResetPasswordBusiness = OnFailureResetPasswordBusiness;

    //Call on Add/Edit business on success
    function OnSuccessResetPasswordBusiness(response) {
        showAlertMessage("#dvNotification", "success", "Business reset password has been sent successfully.");
    }

    //Call on Add/Edit business on failure
    function OnFailureResetPasswordBusiness(XMLHttpRequest, textStatus, errorThrown) { }

    vm.edit = edit;
    vm.cancel = cancel;
    vm.Save = Save;
    vm.clearsearch = clearsearch;
    vm.isNumberKey = isNumberKey;

    vm.ResetPassword = ResetPassword;

    $(document).ready(function () {
        $(".datepicker").datepicker();
        $("#businessSearchListForm").submit();
    });

    function pageSort(url) {
        $("#businessSearchListForm").attr('action', url);
        $("#businessSearchListForm").submit();
    }

    function OnSuccessBindBusinessSearchList() {
        //Hide business on page load.
        $("#trAddEdit").hide();
    }

    function OnFailureBusinessSearchList() {
        alert("ERROR")
    }
    function USDOTNumberLength() {
        var searchByUSDOTNumberString = $('#USDOTNumber').val();
        if (searchByUSDOTNumberString.length < 9) {
            return true;
        }
        else {
            return $('#searchedUSDOTNumber').val("");
        }
    }

    function isNumberKey(evt) {
        var charCode = (evt.which) ? evt.which : event.keyCode;
        if (charCode == 46 && charCode > 31
          && (charCode < 48 || charCode > 57))
            return false;

        return USDOTNumberLength();
    }

    //delete menu
    function DeleteBusiness(BusinessID) {
        if (confirm('Are you sure you want to delete this business?')) {
            $("#BusinessId").val(BusinessID);
            $("#deleteBusinessForm").submit();
        }
    }

    //delete menu
    function ResetPassword(BusinessID) {
        $("#usDOTNumberForResetPassword").val(BusinessID);
        $("#resetPasswordBusinessForm").submit();
    }

    function OnSuccessDeleteBusinessById() {
        showAlertMessage("#dvNotification", "success", "Business deleted successfully.")
        $("#businessSearchListForm").submit();
    }

    function OnFailureDeleteBusinessById()
    { }


    //Call on Add/Edit business on success
    function EditBusinessOnSuccess(response) {
        showAlertMessage("#dvNotification", "success", "Business updated successfully.")
        $("#businessSearchListForm").submit();
        cancel();
    }

    //Call on Add/Edit business on failure
    function EditBusinessOnFailure(XMLHttpRequest, textStatus, errorThrown) { }

    //Add function call when user click on edit button which is in list of business.
    function edit(id) {
        ////first call cancel function to remove or hide edit mode textbox or any error message.
        cancel();
        //Get Tr by id and and set hide displayed current tr
        var trCurrent = $("#" + id);
        trCurrent.hide();
        //Get traddedit textboxes for set value to text box.
        var trAddEdit = $("#trAddEdit");

        //Set Business Id to Update record
        $("#BusinessID").val(id);

        //Set USDOT Number textbox readonly.
        trAddEdit.find("[name='USDOTNumber']").attr('readonly', true);
        //Set USDOT Number and Website Name text boxes value.
        trAddEdit.find("[name='USDOTNumber']").val(trCurrent.data("usdotnumber"));
        trAddEdit.find("[name='WebsiteName']").val(trCurrent.data("website"));
        trAddEdit.find("[name='BusinessContactEmail']").val(trCurrent.data("businesscontactemail"));
        //Email Verified
        if (trCurrent.data("emailverified") == "True") {
            $("#emailVerifiedYes").attr("checked", "checked");
            $("#emailVerifiedYes").click();
            $("#emailVerifiedNo").removeAttr("checked");
        }
        else if (trCurrent.data("emailverified") == "False") {
            $("#emailVerifiedNo").attr("checked", "checked");
            $("#emailVerifiedNo").click();
            $("#emailVerifiedYes").removeAttr("checked");
        }
        else {
            $("#emailVerifiedYes").removeAttr("checked");
            $("#emailVerifiedNo").removeAttr("checked");
        }

        //Website Approved
        if (trCurrent.data("websiteapproved") == "True") {
            $("#websiteApprovedYes").attr("checked", "checked");
            $("#websiteApprovedYes").click();
            $("#websiteApprovedNo").removeAttr("checked");
        }
        else if (trCurrent.data("websiteapproved") == "False") {
            $("#websiteApprovedNo").prop("checked", "checked");
            $("#websiteApprovedNo").click();
            $("#websiteApprovedYes").removeAttr("checked");
        }
        else {
            $("#websiteApprovedYes").removeAttr("checked");
            $("#websiteApprovedNo").removeAttr("checked");
        }

        //Communication Approved
        if (trCurrent.data("communicationapproved") == "True") {
            $("#communicationApprovedYes").attr("checked", "checked");
            $("#communicationApprovedYes").click();
            $("#communicationApprovedNo").removeAttr("checked");
        }
        else if (trCurrent.data("communicationapproved") == "False") {
            $("#communicationApprovedNo").attr("checked", "checked");
            $("#communicationApprovedNo").click();
            $("#communicationApprovedYes").removeAttr("checked");
        }
        else {
            $("#communicationApprovedYes").removeAttr("checked");
            $("#communicationApprovedNo").removeAttr("checked");
        }

        ////display business after current tr
        trAddEdit.insertAfter(trCurrent);
        trAddEdit.show();
        // set focus field to WebsiteName textbox.
        trAddEdit.find("[name='WebsiteName']").focus();
    }

    //Cancel add/Edit business text boxes
    function cancel() {
        $("#tblAddEdit tr").show();
        $("#trAddEdit").hide();
        clearUnobtrusiveValidationMessages("#frmEditBusiness");
    }

    function clearsearch() {
        $("#searchedUSDOTNumber").val("");
        $("#searchedBusinessContactEmail").val("");
        $("#UpdatedAfter").val("");
        $("#ApprovedWebsite").val("Default").prop("checked", "checked");
        $("#businessSearchListForm").submit();
    }

    //Save method to submit Business from to update details
    function Save() {
        var isValidform = false;
        $.validator.unobtrusive.parse("#frmEditBusiness");

        //if Form is valid then only we are submiting the form else we do nothing.
        if ($('#frmEditBusiness').valid() == true) {
            isValidform = true;
        }
        else {
            isValidform = false
        }

        if (isValidform) {
            var businessSearchVM = {};

            //create business model and submit detail using ajax call
            businessSearchVM.BusinessID = $("#BusinessID").val();
            businessSearchVM.Website = $("#WebsiteName").val();
            businessSearchVM.USDOTNumber = $("#USDOTNumber").val();
            businessSearchVM.EmailVerified = $("input[name='EmailVerified']:checked").val() == "Yes" ? true : $("input[name='EmailVerified']:checked").val() == "No" ? false : null;
            businessSearchVM.WebsiteApproved = $("input[name='WebsiteApproved']:checked").val() == "Yes" ? true : $("input[name='WebsiteApproved']:checked").val() == "No" ? false : null;
            businessSearchVM.BusinessContactEmail = $("#BusinessContactEmail").val();
            businessSearchVM.CommunicationApproved = $("input[name='CommunicationApproved']:checked").val() == "Yes" ? true : $("input[name='CommunicationApproved']:checked").val() == "No" ? false : null;

            var url = "/admin/business/save";
            $.ajax({
                type: "POST",
                url: url,
                data: JSON.stringify({
                    businessSearchVM: businessSearchVM
                }),
                contentType: "application/json; charset=utf-8",
                method: 'POST',
                success: function (data) {
                    showAlertMessage("#dvNotification", "success", "Business updated successfully.")
                    $("#businessSearchListForm").submit();
                    cancel();
                }
            });
        }
    }

})(this);