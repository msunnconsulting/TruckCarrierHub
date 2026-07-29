(function (global) {

    var vm = global.vm = {};

    vm.onSuccess = onSuccess;
    vm.onFailure = onFailure;

    function onSuccess() {
        showAlertMessage("#dvNotification", "success", "Statistics settings saved successfully.");
    }

    function onFailure() {}

})(this);
