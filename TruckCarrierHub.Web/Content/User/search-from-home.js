(function (global) {
    "use strict";

    //variables
    var vmHome = global.vmHome = {};
    var selectedValueForSearchTab = "City";
    var isCityFound = true;
    //function
    vmHome.OnSuccessSearchResult = OnSuccessSearchResult;
    vmHome.OnFailureSearchResult = OnFailureSearchResult;
    vmHome.GetSearchResult = GetSearchResult;

    //page initialize
    $(document).ready(function () {
        $("input[type='radio'][name=selectedValue]").click(function () {
            var selectedtText = $(this).val();///User selected value...****
            selectedValueForSearchTab = selectedtText;
            $("#searchText").val('');
            if (selectedtText == "City") {
                $("#searchText").val('');
                $("#searchText").attr("type", "text");
                $("#searchText").attr("placeholder", "Search by City");
            }
            if (selectedtText == "Company Name") {
                $("#searchText").val('');
                $("#searchText").attr("type", "text");
                $("#searchText").attr("placeholder", "Search by Company Name");
            }
            //user select USDOT Number or MC Number then allow Only Number
            if (selectedtText == "USDOT Number") {
                $("#searchText").val('');
                //add css for remove Arrow of type number
                $("#searchText").attr("type", "number");
                $("#searchText").attr("placeholder", "Search by USDOT Number");
            }
            if (selectedtText == "MC Number") {
                $("#searchText").val('');
                $("#searchText").attr("type", "number");
                $("#searchText").attr("placeholder", "Search by MC Number");
            }
        });


        $("#searchText").autocomplete({
            delay: 100,
            minLength: 3,
            source: function (request, response) {
                // Suggest URL
                var suggestURL = "/searchautocomplete";
                //Create Javascript object    
                var searchData = { SelctedSearchPrefix: request.term, SelectedValue: selectedValueForSearchTab };

                if (selectedValueForSearchTab == "City" || selectedValueForSearchTab == "Company Name") {
                    $.ajax({
                        url: suggestURL,
                        data: JSON.stringify(searchData),
                        dataType: "json",
                        type: "POST",
                        contentType: "application/json; charset=utf-8",
                        success: function (data) {
                            if (data.length) {
                                isCityFound = true;
                                response($.map(data, function (item) {
                                    return { label: item.Value, value: item.Value, id: item.Value };
                                }));
                            }
                            else {
                                isCityFound = false;
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
                // JSONP Request               
            }
            , select: function (event, ui) {
                $("#searchText").val(ui.item.value);
                $("#selectedValueForHomepage").val(selectedValueForSearchTab);
                $("#searchCompanyForm").submit();
            }
        }).data("autocomplete")._renderItem = function (ul, item) {
            return $("<li>")
                .data("item.autocomplete", item)
                .append("<a class='auto-complete'><i class='fa fa-building'></i> " + item.label + "<br />" + "</a>")
                .appendTo(ul);
        };;
});

function OnSuccessSearchResult(response) {
    //redirect to response url
    //for city search redirect to company list and for USDOTNumber or MCNumber redirect to Company information page
    window.location.href = response;
}

function OnFailureSearchResult(response, xhr) {
    //if any error occurs then alert this
    var obj = JSON.parse(response.responseText);
    //display dialog box if any error occurs
    ShowDialogBox('Info', obj[0].message, 'Ok', '', '', null);
    }

//if user does not select then display alert box for enter any value
function GetSearchResult() {
    var searchText = $(".home-search-inputbox").val();
    if (!isCityFound && selectedValueForSearchTab == "City") {
        ShowDialogBox('Info', "No city found", 'Ok', '', '', null);
        isCityFound = true;
        $(".home-search-inputbox").val('');
    }
    else if (searchText == "") {
        ShowDialogBox('Info', "Please select search criteria", 'Ok', '', '', null);
        isCityFound = true;
    }
    else {
        //set value of search which user performs like Usdot number or Mc number
        $("#selectedValueForHomepage").val(selectedValueForSearchTab);
        $("#searchCompanyForm").submit();
    }
}

$('#searchText').on('keypress', function (e) {
    if (e.which == 13) {
        GetSearchResult();
        return false;
    }
});

$('#globalHiring').change(function () {
    var checkoxIsCheked = $(this).is(':checked');
    var suggestURL = "/storeischeckcheckboxvalue";
    $.ajax({
        url: suggestURL,
        data: "{ 'isHiringCheckboxCheck': '" + checkoxIsCheked + "'}",
        dataType: "json",
        type: "POST",
        contentType: "application/json; charset=utf-8",
        success: function (data) {
            window.location.href = "/";
        },
        error: function (response) {
            alert(response.responseText);
        },
        failure: function (response) {
            alert(response.responseText);
        }
    });
});


$('#reviews').change(function () {
    var checkoxIsCheked = $(this).is(':checked');
    var suggestURL = "/storereviewsfiltercheckboxvalue";
    $.ajax({
        url: suggestURL,
        data: { isReveiewsCheckboxCheck: checkoxIsCheked },
        type: "POST",
        success: function (data) {
            window.location.href = "/";
        },
        error: function (response) {
            alert(response.responseText);
        },
        failure: function (response) {
            alert(response.responseText);
        }
    });
});
 
}) (this);