(function (global) {
    "use strict";
    var vm = global.vm = {};

    vm.ChangePasswordOnSuccess = ChangePasswordOnSuccess;
    vm.ChangePasswordOnFailure = ChangePasswordOnFailure;
    vm.ResetPasswordOnSuccess = ResetPasswordOnSuccess;
    vm.ResetPasswordOnFailure = ResetPasswordOnFailure;
    vm.ForgotPasswordOnSuccess = ForgotPasswordOnSuccess;
    vm.ForgotPasswordOnFailure = ForgotPasswordOnFailure;
    vm.BusinessLoginOnSuccess = BusinessLoginOnSuccess;
    vm.BusinessLoginOnFailure = BusinessLoginOnFailure;

    //Business Login success/failure function
    function BusinessLoginOnSuccess(response) {
        if (window.opener && !window.opener.closed) {
            window.close();
            window.opener.reviewVm.closeReviewIframe();
            window.location.reload();
        }
        window.location.href = response;
    }

    function BusinessLoginOnFailure(XMLHttpRequse, textStatus, errorThrown) {
    }

    //forgot Password on success/failure function
    function ForgotPasswordOnSuccess(response) {
 
        window.location.href = "/business-forgot-password-success";
    }
    function ForgotPasswordOnFailure(XMLHttpRequse, textStatus, errorThrown) {
    }

    //Reset password success/failure function
    function ResetPasswordOnSuccess(response) {

        window.location.href = "/business-reset-password-success";
    }
    function ResetPasswordOnFailure(XMLHttpRequse, textStatus, errorThrown) {
    }

    //Change password success/failure function
    function ChangePasswordOnSuccess(response) {
        if (response == "") {
            window.location.href = "/edit-business-profile?update=true";
        }
        else {
            window.location.href = "/business/" + response + "?update=true";
        }

    }
    function ChangePasswordOnFailure(XMLHttpRequest, textStatus, erroThrown) {
    }
})(this);