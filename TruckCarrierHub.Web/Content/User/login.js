(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.CreateAccountOnSuccess = CreateAccountOnSuccess;
    vm.CreateAccountOnFailure = CreateAccountOnFailure;

    function CreateAccountOnSuccess(response) {
        window.location.href = "/account-create-successfully";
    }
    function CreateAccountOnFailure(XMLHttpRequest, textStatus, erroThrown) {
    }
})(this);