(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.LoginOnSuccess = LoginOnSuccess;
    vm.LoginOnFailure = LoginOnFailure;
    function LoginOnSuccess(response) {
        window.location.href = response;
    }
    function LoginOnFailure(XMLHttpRequest, textStatus, erroThrown) {
    }
})(this);