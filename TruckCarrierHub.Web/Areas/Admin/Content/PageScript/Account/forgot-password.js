(function (global) {
    "use strict";
    var vm = global.vm = {};
    vm.forgotPasswordOnSuccess = forgotPasswordOnSuccess;
    vm.forgotPasswordOnFailure = forgotPasswordOnFailure;

    function forgotPasswordOnSuccess(response) {
        //window.location.href = "/forgot-password-confirmation"
        showAlertMessage("#dvNotification", "success", "Reset password mail has been sent.")
    }
    function forgotPasswordOnFailure(XMLHttpRequest, textStatus, errorThrown) {
    }

})(this)

