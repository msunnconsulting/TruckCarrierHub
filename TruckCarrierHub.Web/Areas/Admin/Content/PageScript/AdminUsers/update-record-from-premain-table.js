(function (global) {

    //private variables
    var vm = global.vmUpdateRecord = {};
    var progresUpdateTimerId;

    vm.showStartProcessSection = showStartProcessSection;
    vm.onProcessStarted = onProcessStarted;
    vmUpdateRecord.cancelUpdateOnSuccess = cancelUpdateOnSuccess;

    init();
    function init() {
        $(document).ready(function () {
            $("#btnGoBack").hide();
            // check for progress, if already in progress, just show it and keep getting continious progress
            getProgress(function (response) {
                if (response.IsInProgress) {
                    showInProgressProcessSection(response);
                    startGettingProgress();
                } else {
                    showStartProcessSection();
                }
            });
        });
    }

    function startGettingProgress() {
        // keep updating progress every few second
        progresUpdateTimerId = setInterval(updateProcessProgressInUI, 2000);
    }

    function stopGettingProgress() {
        if (progresUpdateTimerId)
            clearTimeout(progresUpdateTimerId); // in case of error no mor progress updates
    }

    function onProcessStarted() {
        startGettingProgress();
    }
     
    function cancelUpdateOnSuccess() {
        stopGettingProgress();
        showStartProcessSection();
        setProgressBar(100);
    }

    function setProgressBar(percentComplete) {
        $("div.progress > div.progress-bar").css({ "width": percentComplete + "%" });
        $("div.progress > div.progress-bar").text(percentComplete + "%");
    }

    function setErrorInformation(errors) {
        if (errors && errors.length > 0) {
            //if any error find then create table and display after process completed
            var errorTable = '<table class="table table-bordered table-responsive table-striped"><tr><th>#</th><th>USDOT Number</th><th>Error Message</th></tr>';
            $.each(errors, function (index, value) {
                errorTable += '<tr><td>' + (index + 1) + '</td><td>' + value.USDOTNumber + '</td><td>' + value.ErrorMessage + '</td></tr>';
            });
            errorTable += '</table>';
            $("#dvErrorNotification").html(errorTable);
            $(".error-info").show();
        }
        else {
            $(".error-info").hide();
            $("#dvErrorNotification").html("");
        }
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
        $(".records-could-not-fetch").text("0");

        // clear errors
        setErrorInformation('');

        $("#btnGoBack").hide();
        $("#dvbtnCancel").show();
    }

    function showInProgressProcessSection(response) {
        // show/hide section
        $("#dvInProgress").show();
        $("#dvNotInProgress").hide();
        // set the progress values
        var percentComplete = Math.round((response.RecordsProcessed / response.RecordsToProcess) * 100, 0);

        if (percentComplete > 100)
            percentComplete = 100;
        percentComplete = (isNaN(percentComplete)) ? 0 : percentComplete;
        setProgressBar(percentComplete);

        // set current records requested by user to process                    
        $(".records-processed").text(response.RecordsProcessed);
        $(".records-to-process").text(response.RecordsToProcess);
        $(".records-failed").text(response.Errors.length);
        $(".records-success").text(response.RecordsSuccessful);
        $(".records-could-not-fetch").text(response.RecordsCouldNotFetch);
        setErrorInformation(response.Errors);
    }

    function getProgress(cb) {
        $.ajax({
            type: "GET",
            url: "/admin/user/process-progress",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                response.RecordsProcessed = (response.RecordsSuccessful + response.Errors.length + response.RecordsCouldNotFetch - response.RecordFailedDuringLatlng);

                if (cb)
                    cb(response);
            },
            error: function (response) {
                console.log("error while getting progress");
                stopGettingProgress();
            }
        });
    }

    function updateProcessProgressInUI() {
        getProgress(function (response) {
            // show latest progresss
            showInProgressProcessSection(response);
            // if process is ended, stop getting futher progress
            if (!response.IsInProgress) {
                stopGettingProgress();
                $("#btnGoBack").show(); // now user can go back to start another process
                $("#dvbtnCancel").hide(); // once process is complete then no need to display cancel button
                if (!response.CheckIsRecordsEnoughToUpdate) {
                    showAlertMessage("#dvNotification", "success", "You've requested to process " + response.TotalRecordCount + " records, but only " + response.TotalCountOfAvailableRecordToUpdate + " records are available to process.");
                }
            }
        });
    }
})(this);