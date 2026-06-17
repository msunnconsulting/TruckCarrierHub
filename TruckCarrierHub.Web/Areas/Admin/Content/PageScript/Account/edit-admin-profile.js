(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.EditAdminOnSuccess = EditAdminOnSuccess;
    vm.EditAdminOnFailure = EditAdminOnFailure;

    function EditAdminOnSuccess(response) {
        showAlertMessage("#dvNotification", "success", "Profile updated successfully.");
    }
    function EditAdminOnFailure(XMLHttpRequest, textStatus, errorThrown) {

    }

})(this)

