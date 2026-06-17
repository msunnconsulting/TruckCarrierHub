(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.ChangePasswordOnSuccess = ChangePasswordOnSuccess;
    vm.ChangePasswordOnFailure = ChangePasswordOnFailure;

    function ChangePasswordOnSuccess(response) {
        showAlertMessage("#dvNotification", "success", "Password changed successfully.")
    }
    function ChangePasswordOnFailure(XMLHttpRequse, textStatus, errorThrown) {

    }
})(this)