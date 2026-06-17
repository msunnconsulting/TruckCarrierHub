(function (global) {

    var vm = global.vm = {};

    vm.OnSuccessGlobalHiring = OnSuccessGlobalHiring;
    vm.OnFailureGlobalHiring = OnFailureGlobalHiring;

    //On Success Global Hiring
    function OnSuccessGlobalHiring() {
        showAlertMessage("#dvNotification", "success", "Global Hiring updated successfully.");
    }

    //Call on  Update Success Story on failure
    function OnFailureGlobalHiring(XMLHttpRequest, textStatus, errorThrown) { }

    
})(this);