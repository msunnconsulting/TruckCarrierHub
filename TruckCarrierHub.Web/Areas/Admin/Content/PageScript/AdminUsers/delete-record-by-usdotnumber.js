(function (global) {

    var vm = global.vm = {};
    vm.OnSuccessDeleteRecord = OnSuccessDeleteRecord;
    vm.OnFailureDeleteRecord = OnFailureDeleteRecord;


    function OnSuccessDeleteRecord(response)
    {
        $("#usdotNumber-textbox").val('');
        showAlertMessage("#dvNotification", "success", "Record deleted successfully.");
    }

    function OnFailureDeleteRecord()
    { }

})(this);