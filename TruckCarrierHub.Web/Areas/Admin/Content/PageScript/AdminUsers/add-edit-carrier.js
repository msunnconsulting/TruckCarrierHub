(function (global) {

    var vm = global.vm = {};
    vm.OnSuccessAddUpdateCarrier = OnSuccessAddUpdateCarrier;
    vm.OnFailureAddUpdateCarrier = OnFailureAddUpdateCarrier;

    $(document).ready(function () {

        //validation for checkbox list select at least one checkbox from checkbox list
        $.validator.addMethod("LoadType", function (value, element) {
            return $('.LoadType:checked').length > 0;
        }, 'Select at least one Load Type');

        $.validator.addMethod("PickupStateCode", function (value, element) {
            return $('.PickupStateCode:checked').length > 0;
        }, 'Select at least one Pickup Location');

        $.validator.addMethod("DeliveryStateCode", function (value, element) {
            return $('.DeliveryStateCode:checked').length > 0;
        }, 'Select at least one Delivery Location');

    });

    //On Success of Add/Update Carrier
    function OnSuccessAddUpdateCarrier() {
        var id = $("#Id").val();
        if (id == '' || id == undefined) {
            window.location.href = "/admin/business/manage-carrier?create=true"
        }
        else {
            window.location.href = "/admin/business/manage-carrier?update=true"
        }
    }

    //Call on Add/Edit carrier on failure
    function OnFailureAddUpdateCarrier(XMLHttpRequest, textStatus, errorThrown) { }

})(this);