(function (global) {

    var vm = global.vm = {};

    var progresUpdateTimerId;


    vm.OnSuccessEmailList = OnSuccessEmailList;
    vm.OnFailureEmailList = OnFailureEmailList;


    vm.OnBeginSendEmail = OnBeginSendEmail;
    vm.OnSuccessSendEmail = OnSuccessSendEmail;



    vm.showStartProcessSection = showStartProcessSection;
    vm.onProcessStarted = onProcessStarted;
    vm.cancelSendMailOnSuccess = cancelSendMailOnSuccess;

    init();
    function init() {
        $(document).ready(function () {

            //Get parameter value using query string
            var Update = getUrlParameter('update');
            var Create = getUrlParameter('create');
            //if parameter is create then display create message
            if (Create != undefined && Create != '' && Create == "true") {
                showAlertMessage("#dvNotification", "success", "Email added successfully.")
            }

            //if parameter is update then display update message
            if (Update != undefined && Update != '' && Update == "true") {
                showAlertMessage("#dvNotification", "success", "Email updated successfully.")
            }
            $(".datepicker").datepicker();

            $("#State").change(function () {
                var stateCode = $(this).val(); // 
                $('#City').empty().append('<option selected="selected">--Select--</option>');
                if (stateCode != '' && stateCode != undefined) {
                    $.ajax({
                        url: "/admin/business/get-cities-by-state-code/" + stateCode,
                        type: "Get",
                        success: function (data) {
                            for (var i = 0; i < data.length; i++) {
                                var opt = new Option(data[i].CityName, data[i].CityName);
                                $('#City').append(opt);
                            }
                        },
                        error: function (response) {
                            alert(response.responseText);
                        },
                        failure: function (response) {
                            alert(response.responseText);
                        }
                    });
                }
            });

            getProgress(function (response) {
                if (response.IsInProgress) {
                    $("#btnStartEmail").hide();
                    $("#btnStopEmail").show();

                    showInProgressProcessSection(response);
                    startGettingProgress();
                } else {
                    showStartProcessSection();
                }
            });



        });
    }

    function cancelSendMailOnSuccess() {
        var emailId = $("#EmailID").val();
        $.ajax({
            type: "GET",
            url: "/admin/business/stop-email-sent-progress/" + emailId,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                stopGettingProgress();
                showStartProcessSection();
                setProgressBar(100);
                $("#btnStartEmail").show();
                $("#btnStopEmail").hide();
            },
            error: function (response) {
                console.log("error while getting progress");
                stopGettingProgress();
                $("#btnStartEmail").show();
                $("#btnStopEmail").hide();
            }
        });

    }

    function getProgress(cb) {
        var emailId = $("#EmailID").val();
        $.ajax({
            type: "GET",
            url: "/admin/business/email-sent-progress/" + emailId,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                response.RecordsProcessed = (response.MailSentSuccessful + response.MailSentFailed);
                if (cb)
                    cb(response);
            },
            error: function (response) {
                console.log("error while getting progress");
                stopGettingProgress();
            }
        });
    }

    function onProcessStarted() {
        startGettingProgress();
    }

    function showInProgressProcessSection(response) {
        // show/hide section
        $("#dvInProgress").show();
        $("#dvNotInProgress").hide();


        // set the progress values
        var percentComplete = Math.round((response.RecordsProcessed / response.TotalMailToSent) * 100, 0);

        if (percentComplete > 100)
            percentComplete = 100;
        percentComplete = (isNaN(percentComplete)) ? 0 : percentComplete;
        setProgressBar(percentComplete);

        // set current records requested by user to process                    
        $(".records-processed").text(response.RecordsProcessed);
        $(".records-to-process").text(response.TotalMailToSent);
        $(".records-success").text(response.MailSentSuccessful);
        $(".records-failed").text(response.MailSentFailed);

        $("#real-time-counter").text(response.RealTimeCounter);

        //setErrorInformation(response.Errors);
    }

    function cancelUpdateOnSuccess() {
        stopGettingProgress();
        showStartProcessSection();
        setProgressBar(100);
    }
    function startGettingProgress() {
        // keep updating progress every few second
        progresUpdateTimerId = setInterval(updateProcessProgressInUI, 2000);
    }

    function stopGettingProgress() {
        if (progresUpdateTimerId)
            clearTimeout(progresUpdateTimerId); // in case of error no mor progress updates
    }


    function showStartProcessSection() {
        // show/hide section
        $("#dvInProgress").hide();
        $("#dvNotInProgress").show();

        // set the progress values
        setProgressBar(0);

        // clear inputbox for number of records to process
        $("#recordsToProcess").val("");

        // set all records count to 0
        $(".records-processed").text("0");
        $(".records-to-process").text("0");
        $(".records-failed").text("0");
        $(".records-success").text("0");

        // clear errors
        //setErrorInformation('');

        //$("#btnStartEmail").hide(); // now user can go back to start another process
        //$("#btnStopEmail").show(); // once process is complete then no need to display cancel button
    }


    function setProgressBar(percentComplete) {
        $("div.progress > div.progress-bar").css({ "width": percentComplete + "%" });
        $("div.progress > div.progress-bar").text(percentComplete + "%");
    }


    function updateProcessProgressInUI() {
        getProgress(function (response) {
            // show latest progresss
            showInProgressProcessSection(response);
            // if process is ended, stop getting futher progress
            if (!response.IsInProgress) {
                stopGettingProgress();
                $("#btnStartEmail").show(); // now user can go back to start another process
                $("#btnStopEmail").hide(); // once process is complete then no need to display cancel button

                if (!response.CheckIsRecordsEnoughToUpdate) {
                    showAlertMessage("#dvNotification", "success", "You've requested to process " + response.TotalMailToSent + " records, but only " + response.TotalRecordCountForMail + " records are available to process.");
                }
                else {
                    showAlertMessage("#dvNotification", "success", "Mail sent successfully.");
                }
            }
        });
    }
    function OnSuccessEmailList() { }

    //Call on Add/Edit business on failure
    function OnFailureEmailList(XMLHttpRequest, textStatus, errorThrown) { }





    function OnBeginSendEmail() {
        $("#btnStartEmail").hide();
        $("#btnStopEmail").show();

    }
    function OnSuccessSendEmail() {
    }
    function OnFailureSendEmail(XMLHttpRequest, textStatus, errorThrown) { }




})(this);