(function (global) {

    var vm = global.vm = {};

    vm.pageSort = pageSort;
    vm.OnSuccessBindCleanCompanyMonthlyList = OnSuccessBindCleanCompanyMonthlyList;
    vm.OnFailureBindCleanCompanyMonthlyList = OnFailureBindCleanCompanyMonthlyList;

    vm.CleanAllCompanyEveryMonth = CleanAllCompanyEveryMonth;
    vm.OnSuccessCleaAllCompanyEveryMonth = OnSuccessCleaAllCompanyEveryMonth;
    vm.OnFailureCleaAllCompanyEveryMonth = OnFailureCleaAllCompanyEveryMonth;

    vm.OnSuccessClearSelectedCompanyEveryMonth = OnSuccessClearSelectedCompanyEveryMonth;
    vm.OnFailureClearSelectedCompanyEveryMonth = OnFailureClearSelectedCompanyEveryMonth;

    vm.updateCheckboxState = updateCheckboxState;
    vm.ChangeSelectAll = ChangeSelectAll;
    vm.CleanSelectedCompanyEveryMonth = CleanSelectedCompanyEveryMonth;
    var selectedUSDOTNumber = [];

    $(document).ready(function () {
        $("#cleanCompanyMonthlyListForm").submit();
    });

    //Cal on success
    function OnSuccessBindCleanCompanyMonthlyList(response) {
        if ($("#isAllCheckboxCheck").is(':checked')) {
            $("#CommaSeparatedValue").val("");
            selectedUSDOTNumber.length = 0;
            $(".checkbox").prop("checked", true);
        }
        else {
            //check only selected checkboxes
            $('input[type=checkbox]').filter(function () {
                return $.inArray(this.value, selectedUSDOTNumber) > -1;
            }).prop('checked', true);
        }

        var cleared = getUrlParameter('cleared');
        if (cleared != "" && cleared != undefined && cleared != true) {
            showAlertMessage("#dvNotification", "success", "Cities cleared successfully.");
        }

    }

    //Call on failure
    function OnFailureBindCleanCompanyMonthlyList(XMLHttpRequest, textStatus, errorThrown) { }

    function pageSort(url) {
        $("#CommaSeparatedUSDOTNumber").val(selectedUSDOTNumber);
        $("#cleanCompanyMonthlyListForm").attr('action', url);
        $("#cleanCompanyMonthlyListForm").submit();
    }

    //Clean All company from Business then from TransportCompany table
    function CleanAllCompanyEveryMonth() {
        if (confirm('Are you sure you want to clear all cities with 1 company?')) {
            $("#cleanCompanyMonthlyFrm").submit();
        }
    }

    function OnSuccessCleaAllCompanyEveryMonth() {
        $("#cleanCompanyMonthlyListForm").submit();
        showAlertMessage("#dvNotification", "success", "Cities cleared successfully.")
    }

    function OnFailureCleaAllCompanyEveryMonth() {
    }

    function updateCheckboxState(state) {
        $(".checkboxAll").prop("checked", false);
        $("#isAllCheckboxCheck").val(false);
        //Getting Checkbox selected ids for USDOTNUmber
        //var selectedUSDOTNumber = [];
        if ($('#selectAll').is(':checked')) {
            selectedUSDOTNumber.length = 0;
        }

        //Update selectedUSDOTNumber array when if selected checkbox then check it's available usdotnumber in array if not then add it 
        //if checkbox is uncheck then check SelectedUSDOTNUmber array contain that USDOT number then remove that USDOTNumber from that array.
        //Update array based on selelction of checkbox if selected checkbox then add to array and uncheck then renmove from array if it available in array.
        $.each($("input[name='USDOTNumber']"), function () {
            if (this.checked) {
                selectedUSDOTNumber.indexOf(this.value) === -1 ? selectedUSDOTNumber.push($(this).attr("value")) : selectedUSDOTNumber;
            }
            else {
                var index = selectedUSDOTNumber.indexOf(this.value);
                if (index > -1) {
                    selectedUSDOTNumber.splice(index, 1);
                }
            }
        });

        //Update value of checked UsDotNumber checkbox values in commma separated values
        $("#CommaSeparatedCheckUSDOTNumber").val(selectedUSDOTNumber);

    }

    function ChangeSelectAll() {
        selectedUSDOTNumber.length = 0;
        if ($('#selectAll').is(':checked')) {
            $("#isAllCheckboxCheck").val($('#selectAll').is(':checked'));
            $(".checkbox").prop("checked", true);
            $("#CommaSeparatedUSDOTNumber").val("");
        }
        else {
            $(".checkbox").prop("checked", false);
        }
    }

    function CleanSelectedCompanyEveryMonth() {
        if ($("input[name='USDOTNumber']").is(':checked')) {
            if (confirm('Are you sure you want to clear selected cities?')) {
                $("#DeleteSeletedUSDOTNumber").val(selectedUSDOTNumber);
                $("#cleanSelectedCompanyMonthlyFrm").submit();
                selectedUSDOTNumber.length = 0;
            }
        }
    }

    function OnSuccessClearSelectedCompanyEveryMonth() {
        window.location.href = '/admin/user/clean-company-monthly-list?cleared=true';
    }

    function OnFailureClearSelectedCompanyEveryMonth() {

    }

})(this);