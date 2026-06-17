(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.SaveBusinessOnSuccess = SaveBusinessOnSuccess;
    vm.SaveBusinessOnFailure = SaveBusinessOnFailure;


    vm.LoginOnSuccess = LoginOnSuccess;
    vm.LoginOnFailure = LoginOnFailure;

    function SaveBusinessOnSuccess(response) {
        window.location.href = response;
    }
    function SaveBusinessOnFailure(XMLHttpRequest, textStatus, erroThrown) {
    }
    function LoginOnSuccess(response) {
        window.location.href = response;
    }
    function LoginOnFailure(XMLHttpRequest, textStatus, erroThrown) {
    }
})(this);