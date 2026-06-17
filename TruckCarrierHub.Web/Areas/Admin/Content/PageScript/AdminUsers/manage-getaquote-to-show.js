(function (global) {

    var vm = global.vm = {};

    vm.OnSuccessGetAQuoteToShow = OnSuccessGetAQuoteToShow;
    vm.OnFailureGetAQuoteToShow = OnFailureGetAQuoteToShow;

    //On Success Get A Quote To Show updated successfully
    function OnSuccessGetAQuoteToShow() {
        showAlertMessage("#dvNotification", "success", "Get A Quote to show updated successfully.");
    }

    //Call on  Update Get A Quote To Show on failure
    function OnFailureGetAQuoteToShow(XMLHttpRequest, textStatus, errorThrown) { }
   
    
})(this);