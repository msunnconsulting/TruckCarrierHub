(function (global) {

    var vm = global.vm = {};
    vm.OnSuccessDeleteReviews = OnSuccessDeleteReviews;
    vm.OnFailureDeleteReviews = OnFailureDeleteReviews;


    function OnSuccessDeleteReviews(response)
    {
        $("#usdotNumber-textbox").val('');
        showAlertMessage("#dvNotification", "success", "Reviews deleted successfully.");
    }

    function OnFailureDeleteReviews()
    { }

})(this);