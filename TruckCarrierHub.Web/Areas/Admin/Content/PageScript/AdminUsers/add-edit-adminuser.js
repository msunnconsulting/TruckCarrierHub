(function (global) {
    "use strict";

    var vm = global.vm = {};

    //function
    vm.OnSuccessAddUpdateAdmin = OnSuccessAddUpdateAdmin;
    vm.OnFailureAddUpdateAdmin = OnFailureAddUpdateAdmin;
    vm.ChangePassword = ChangePassword;
    $(document).ready(function () {
        var a = $("#adminID").val();
        if ($("#adminID").val() != "") {

            $(".changePassword").hide();
            $("#password").val($("#currentPassword").val());
            $("#confirmPassword").val($("#currentPassword").val());
        }
    });
    //After Successfull add adminuser
    function OnSuccessAddUpdateAdmin(response) {
        //maintain pagination
        var p = $("#pageSortPara_p").val();
        var se = $("#pageSortPara_se").val();
        var sd = $("#pageSortPara_sd").val();
        var c = $("#adminID").val();
        if ($("#adminID").val() == '')
            window.location.href = '/admin/user/list?create=true';
        else
            window.location.href = '/admin/user/list?update=true&p=' + p + '&se=' + se + '&sd=' + sd;
    }

    function ChangePassword() {
        var checklinkText = $(".changePasswordLink").text();
        if (checklinkText == "Don't Change Password") {
            $(".changePassword").hide();
            $("#password").val($("#currentPassword").val());
            $("#confirmPassword").val($("#currentPassword").val());
            $(".changePasswordLink").text("Change Password")
        }
        else {
            $("#confirmPassword").val(null);
            $("#password").val(null);
            $(".changePassword").show();
            $(".changePasswordLink").text("Don't Change Password");
        }

    }

    function OnFailureAddUpdateAdmin(XMLHttpRequest, textStatus, errorThrown) {
    }
})(this);
