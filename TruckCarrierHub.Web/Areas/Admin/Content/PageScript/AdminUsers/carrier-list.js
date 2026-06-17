(function (global) {

    var vm = global.vm = {};
    vm.pageSort = pageSort;
    vm.OnSuccessBindCarrierList = OnSuccessBindCarrierList;
    vm.OnFailureBindCarrierList = OnFailureBindCarrierList;

    vm.InactivateCarrier = InactivateCarrier;
    vm.ActivateCarrier = ActivateCarrier;
    vm.OnSuccessActivateInActivateById = OnSuccessActivateInActivateById;
    vm.OnFailureActivateInActivateById = OnFailureActivateInActivateById;

    vm.clearsearch = clearsearch;
    vm.submit = submit;


    $(document).ready(function () {
        //Get parameter value using query string
        var Update = getUrlParameter('update');
        var Create = getUrlParameter('create');

        //if parameter is create then display create message
        if (Create != undefined && Create != '' && Create == "true") {
            showAlertMessage("#dvNotification", "success", "Carrier created successfully.")
        }

        //if parameter is update then display update message
        if (Update != undefined && Update != '' && Update == "true") {
            showAlertMessage("#dvNotification", "success", "Carrier updated successfully.")
        }

        $("#carrierListForm").submit();
    });

    function pageSort(url) {
        $("#carrierListForm").attr('action', url);
        $("#carrierListForm").submit();
    }

    function OnSuccessBindCarrierList() {
    }

    function OnFailureBindCarrierList() {
        alert("ERROR")
    }

    function clearsearch() {
        $("#searchCompanyName").val("");
        $("#searchEmail").val("");
        $("#searchPhoneNumber").val("");
        $("#carrierListForm").submit();
    }
    function submit() {
        var companyname = $("#searchCompanyName").val();
        var email = $("#searchEmail").val();
        var phonenumber = $("#searchPhoneNumber").val();
        
        if (companyname != "" || email != ""  || phonenumber != "") {
            $("#carrierListForm").submit();
        }
    }

    //Activate/Inactivate Carrier function
    //Activate Function
    function ActivateCarrier(Id) {
        //set Activate Id and submit form
        $('#carrierId').val(Id);
        $("#carrierActivateInactivateForm").submit();
        
        showAlertMessage("#dvNotification", "success", "Carrier Activated successfully.");
    }

    //Inactivate Function
    function InactivateCarrier(Id) {
        //set Inactivate Id and submit frmactivatdeactivate
        $('#carrierId').val(Id);
        $("#carrierActivateInactivateForm").submit();
        $("#carrierListForm").submit();
        showAlertMessage("#dvNotification", "success", "Carrier InActivated successfully.");
    }

    function OnSuccessActivateInActivateById() {
        $("#carrierListForm").submit();
    }

    function OnFailureActivateInActivateById() {
        alert("ERROR")
    }

     

})(this);