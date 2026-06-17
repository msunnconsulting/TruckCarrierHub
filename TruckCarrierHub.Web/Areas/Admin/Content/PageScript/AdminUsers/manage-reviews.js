(function (global) {

    var vm = global.vm = {};

    vm.onSuccessReviewsFilterUpdate = onSuccessReviewsFilterUpdate;
    vm.onFailureReviewsFilterUpdate = onFailureReviewsFilterUpdate;

    // On success: Reviews Filter update
    function onSuccessReviewsFilterUpdate() {
        showAlertMessage("#dvNotification", "success", "Review filter settings updated successfully.");
    }

    // On failure: Reviews Filter update
    function onFailureReviewsFilterUpdate(XMLHttpRequest, textStatus, errorThrown) { }

})(this);
