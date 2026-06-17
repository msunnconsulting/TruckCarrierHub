(function (global) {
    var vm = global.vm = {};

    vm.resetPasswordOnSuccess = resetPasswordOnSuccess;
    vm.resetPasswordOnFailure = resetPasswordOnFailure;
    function resetPasswordOnSuccess(response) {
        window.location.href = "/admin/account/reset-password-confirmation";
    }
    function resetPasswordOnFailure(XMLHttpRequest, textStatus, thrownError) {

    }
})(this)