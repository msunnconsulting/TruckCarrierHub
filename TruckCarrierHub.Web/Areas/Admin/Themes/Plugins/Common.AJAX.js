/// <reference path="jquery-1.10.2.min.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="jquery.serializejson.js" />

// usually we use following fields - but extensive list of properties of settings object can be found here: http://api.jquery.com/jquery.ajax/
//settings = {
//    url,
//    type, // type of request : GET, POST. Default. GET
//    contentType, // type of data being sent to server. default: 'application/x-www-form-urlencoded; charset=UTF-8'. for json 'application/json', for html
//    dataType, // typeof data expected back from sever : html, json, xml etc. Default : Intelligent Guess
//    data, // data that is sent to server. In case of GET request, it would be sent in query string
//    async, // default : true. false will stop execution of further javascript in code until response is back
//    success, // success callback
//    error,  // error callback,
//   complete // callback that gets executed after success/error execution is completed
//}
$(document).ajaxStart(function () { ShowSpinner(); });
$(document).ajaxComplete(function () { HideSpinner(); });
$(document).ajaxSuccess(function (event, xhr, settings) {
    HideSpinner();
    //show messages from response in toastr

    if (xhr.responseJSON != undefined && xhr.responseJSON.AutoShowMessages != undefined && xhr.responseJSON.AutoShowMessages != null && xhr.responseJSON.AutoShowMessages.length > 0) {
        for (var i = 0; i <= xhr.responseJSON.AutoShowMessages.length - 1; i++) {

            //toastr option set as response
            toastr.options = {
                "debug": false,
                "onclick": null,
                "showDuration": "300",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut",
                "closeButton": xhr.responseJSON.AutoShowMessages[i].ShowClose,
                "positionClass": xhr.responseJSON.AutoShowMessages[i].PositionName,
            }

            //check type name and show message respectively
            if (xhr.responseJSON.AutoShowMessages[i].TypeName == "error") {

                toastr.error(xhr.responseJSON.AutoShowMessages[i].Message, xhr.responseJSON.AutoShowMessages[i].Title)
            }
            if (xhr.responseJSON.AutoShowMessages[i].TypeName == "success") {

                toastr.success(xhr.responseJSON.AutoShowMessages[i].Message, xhr.responseJSON.AutoShowMessages[i].Title)
            }
            if (xhr.responseJSON.AutoShowMessages[i].TypeName == "warning") {

                toastr.warning(xhr.responseJSON.AutoShowMessages[i].Message, xhr.responseJSON.AutoShowMessages[i].Title)
            }
            if (xhr.responseJSON.AutoShowMessages[i].TypeName == "info") {

                toastr.info(xhr.responseJSON.AutoShowMessages[i].Message, xhr.responseJSON.AutoShowMessages[i].Title)
            }
        }
    }
});
$(document).ajaxStop(function () { HideSpinner() });
$(document).ajaxError(function (e, xhr, settings, thrownError) {

   

    HideSpinner();
    if (thrownError.toLowerCase() != "abort") {
        if (thrownError == undefined || thrownError == null || thrownError == "") {
            thrownError = "Internal AJAX Error : Unidentified AJAX Error Occurred";
        }
        //else
        //thrownError = "Internal AJAX Error : " + thrownError;

        if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == "Forbidden") || (thrownError == "Access Denied")) {
            thrownError = "It seems your session is expired or you are not authorized to access the resource you are trying to use.";
        
            window.location.reload();
            return;
        }
        else if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == "Bad Request") || (thrownError == "Conflict")) {
            var parsedJson = JSON.parse(xhr.responseText)
            thrownError = '';
            for (var i = 0; i < parsedJson.length; i++) {

                thrownError += parsedJson[i].message + '<br/>';
            }
        }
        else if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == "canceled") || (thrownError == "canceled")) {
            return;
        }
        else if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == "Internal Server Error") || (thrownError == "Internal Server Error")) {
            window.location.href = "/admin/account/error"
            return;
        }
       


        else if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == "Unauthorized") || (thrownError == "NotAuthorized")) {
            if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == unauthorizedMessage) || (thrownError == "Forbidden")) {
                thrownError = "It seems your session is expired or you are not authorized to access the resource you are trying to use.";
                toastr.error(thrownError);
                window.location.reload();
                return;
            }
        }
        else {
            if ((xhr != undefined && xhr.statusText != undefined && xhr.statusText == "Bad Request") || (thrownError == "Forbidden")) {
                thrownError = "It seems your session is expired or you are not authorized to access the resource you are trying to use.";
                toastr.error(thrownError);
                window.location.reload();
                return;
            }
        }
         
        //Check condition that if this two message is there then that means its come from claim controller so we display search error message in search partial view.
        if (thrownError.indexOf("No request number found") >= 0 || thrownError.indexOf("No payment number found") >= 0) {
            //Show message in alert message box.
            showAlertMessage("#dvClaimNotification", "danger", thrownError)
        }
        else {
            //Show message in alert message box.
            showAlertMessage("#dvNotification", "danger", thrownError)
        }

        
    }

});

$(document).ajaxSend(function (evt, request, settings) {
    if (!(settings.url.indexOf("searchautocomplete") >= 0)) {
        ShowSpinner();
    }
    else {
        HideSpinner();
    }
});

function ShowSpinner() {
    var freezeDiv = $('#disabledpage');
    freezeDiv.addClass("disablescreen");
    $("#al").show();
}
function HideSpinner() {
    $("#al").hide();
    var freezeDiv = $('#disabledpage');
    freezeDiv.removeClass("disablescreen");
}

 
 // clears unobtrusive validation messages.
function clearUnobtrusiveValidationMessages(formSelector) {
    $(formSelector + " .field-validation-error").empty();
}
 

function showAlertMessage(divid, errorType, message) {
    $(divid).show();
    var errmsgstr = ' <div class="col-md-12"><div class="alert alert-' + errorType + ' alert-dismissable"><a href="#" class="close" data-dismiss="alert" aria-label="close">&times;</a> ' + message + ' </div></div>'
 
    $(divid).empty();
    $(divid).append(errmsgstr).hide().show('normal');
     
}

//Create function for get value from query string.
function getUrlParameter(sParam) {
    var sPageURL = decodeURIComponent(window.location.search.substring(1)),
        sURLVariables = sPageURL.split('&'),
        sParameterName,
        i;
    for (i = 0; i < sURLVariables.length; i++) {
        sParameterName = sURLVariables[i].split('=');
        if (sParameterName[0] === sParam) {
            return sParameterName[1] === undefined ? true : sParameterName[1];
        }
    }
};

function ShowDialogBox(title, content, btn1text, btn2text, functionText, parameterList) {
    var btn1css;
    var btn2css;
    if (btn1text == '') {
        btn1css = "hidecss";
    } else {
        btn1css = "showcss btn btn-primary";
    }
    $("#lblMessage").html(content);
    $("#dialog").dialog({
        resizable: false,
        title: false,
        modal: true,
        width: 'auto',
        height: 'auto',
        bgiframe: false,
        position: 'top',
        hide: { effect: 'fade', duration: 400 },
        buttons: [
                        {
                            text: btn1text,
                            "class": btn1css,
                            click: function () {
                                $("#dialog").dialog('close');
                            }
                        },
        ]
    });
    $(".ui-dialog-titlebar").hide();
}