(function (global) {
    "use strict";
    //variable
    var vmLayout = global.vmLayout = {};

    //functions
     vmLayout.searchResult = searchResult;
    vmLayout.OnSuccessSearchResult = OnSuccessSearchResult;
    vmLayout.OnFailureSearchResult = OnFailureSearchResult;

    vmLayout.submit = submit;
    vmLayout.OnSuccessGetAQuote = OnSuccessGetAQuote;
    vmLayout.OnFailureGetAQuote = OnFailureGetAQuote;

    var selectedValue = "City";
    var isCityFound = true;
    $(document).ready(function () {
        $('body').on("click", ".dropdowntoggle-autoclose", function (e) {
            $(this).parent().is(".open") && e.stopPropagation();
        });
        $('body').removeClass('open');
        // Scoped to #dropdown-change (this navbar's own City/Company Name/USDOT/MC search-type
        // dropdown) - it used to be the far broader ".dropdown-menu li a", which also matched
        // every OTHER .dropdown-menu on the page (e.g. City page's Cargo/Entity/Sort By filter
        // dropdowns). It also never called preventDefault(), so each click followed its
        // href="#" and changed the URL; on City pages specifically that combines badly with
        // this same file's window.onpopstate/pageshow reload handlers, causing an unwanted
        // page reload that resets the module-scoped selectedValue below back to its "City"
        // default - which is why any other search type appeared to silently revert to City.
        $("#dropdown-change li a").click(function (e) {
            e.preventDefault();
            var selectedtText = $(this).text();///User selected value...****
            selectedValue = selectedtText;
            if (selectedtText == "City") {
                $("#searchTextBox").val('');
                $("#searchTextBox").attr("type", "text");
                $("#searchTextBox").attr("placeholder", "Search by City");
            }
            if (selectedtText == "Company Name") {
                $("#searchTextBox").val('');
                $("#searchTextBox").attr("type", "text");
                $("#searchTextBox").attr("placeholder", "Search by Company Name");
            }
                //user select USDOT Number or MC Number then allow Only Number
            if (selectedtText == "USDOT Number") {
                $("#searchTextBox").val('');
                //add css for remove Arrow of type number
                $("#searchTextBox").attr("type", "number");     
                $("#searchTextBox").attr("placeholder", "Search by USDOT Number");
            }
            if (selectedtText == "MC Number"){
                $("#searchTextBox").val('');
                $("#searchTextBox").attr("type", "number");
                $("#searchTextBox").attr("placeholder", "Search by MC Number");
            }
            //window.onpopstate = () => { };
            window.addEventListener('popstate', function (event) {
                if (event.state) {
                    //alert('!');
                }
            }, false);
        });
        var _navIsFixed = false;
        $(window).on('scroll', function () {
            var shouldFix = $(window).scrollTop() > 0;
            if (shouldFix === _navIsFixed) { return; }
            _navIsFixed = shouldFix;
            if (shouldFix) {
                $('#mainnav').addClass('navbar-fixed-top');
            } else {
                $('#mainnav').removeClass('navbar-fixed-top');
            }
        });

        $("#searchTextBox").autocomplete({
            delay: 100,
            minLength: 3,
            source: function (request, response) {
                // Suggest URL
                var suggestURL = "/searchautocomplete";
                //Create Javascript object    
                var searchData = { SelctedSearchPrefix: request.term, SelectedValue: selectedValue };

                if (selectedValue == "City" || selectedValue == "Company Name") {
                    // JSONP Request
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
                                //if no city found then it will false
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
              
            }
            , select: function (event, ui) {
                $("#searchTextBox").val(ui.item.value);
                $("#selectedValue").val(selectedValue);
                $("#searchForm").submit();
            }
        }).data("autocomplete")._renderItem = function (ul, item) {
            return $("<li>")
                .data("item.autocomplete", item)
                .append("<a class='auto-complete'><i class='fa fa-building'></i> " + item.label + "<br />" + "</a>")
                .appendTo(ul);
        };;
    });
    function searchResult() {
        var searchparam = $("#selectedValue").val(selectedValue);
        var searchText = $("#searchTextBox").val();
        if (!isCityFound && selectedValue == "City")
        {   //display pop up for no city found
            ShowDialogBox('Info', "No city found", 'Ok', '', '', null);
            isCityFound = true;
            $("#searchTextBox").val('');
        }
         else if (searchText == "") {
             ShowDialogBox('Info', "Please select search criteria", 'Ok', '', '', null);
             isCityFound = true;
        }
        else {
            $("#searchForm").submit();
        }
    }

    $('#searchTextBox').on('keypress', function (e) {
        if (e.which == 13 && $(this).val()) {
            searchResult();
            return false;
        }
    });

    function OnSuccessSearchResult(response) {
        window.location.href = response;
    }

    function OnFailureSearchResult(response) {
        var obj = JSON.parse(response.responseText);
        //display dialog box if any error occurs
        ShowDialogBox('Info', obj[0].message, 'Ok', '', '', null);
    }

    var StatePage = $("#fromStatePage").val();
    //if from State page then only this change function works
    if (StatePage) {
        $('#globalHiring').change(function () {
            var checkoxIsCheked = $(this).is(':checked');
            var suggestURL = "/storeischeckcheckboxvalue";
            var getStateFromURL = window.location.href.split('/');
            $.ajax({
                url: suggestURL,
                data: "{ 'isHiringCheckboxCheck': '" + checkoxIsCheked + "'}",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (getStateFromURL[3] == "" && getStateFromURL[3] == undefined) {
                        window.location.href = "/";
                    }
                    else {
                        window.location.href = "/" + getStateFromURL[3];
                    }
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
            var getStateFromURL = window.location.href.split('/');
            $.ajax({
                url: suggestURL,
                data: { isReveiewsCheckboxCheck: checkoxIsCheked },
                type: "POST",
                success: function (data) {
                    if (getStateFromURL[3] == "" && getStateFromURL[3] == undefined) {
                        window.location.href = "/";
                    }
                    else {
                        window.location.href = "/" + getStateFromURL[3];
                    }
                },
                error: function (response) {
                    alert(response.responseText);
                },
                failure: function (response) {
                    alert(response.responseText);
                }
            });
        });
    }
    //Submit Get a quote control 
    function submit() {
        if ($("#PickupLocation").val() != "" && $("#DeliveryLocation").val() != "") {
            $("#getAQuoteForm").submit();
        } else {
            if ($("#PickupLocation").val() == "" && $("#DeliveryLocation").val() == "") {
                ShowDialogBox('Info', "Pickup Location & Delivery Location field is required", 'Ok', '', '', null);
            }
            else {
                if ($("#PickupLocation").val() == "") {
                    ShowDialogBox('Info', "Pickup Location field is required", 'Ok', '', '', null);
                }
                else {
                    ShowDialogBox('Info', "Delivery Location field is required", 'Ok', '', '', null);
                }
            }
        }
    }
    //On success for Submit Get a quote control
    function OnSuccessGetAQuote(response) {
        var newUrl = window.location.origin + "/" + response.url;
        window.location.href = newUrl;
    }

    //On failure for Submit Get a quote control
    function OnFailureGetAQuote() {
    }

 var isDeliverySelected = false;
// Get A Quote widget is admin-toggleable per page (e.g. homepage "Show on homepage" flag),
// so #DeliveryLocation may not exist in the DOM on every page this script runs on. Guard
// against that instead of throwing on an empty jQuery selection.
if ($("#DeliveryLocation").length) {
$("#DeliveryLocation").autocomplete({
    delay: 100,
    minLength: 3,
    source: function (request, response) {
        // Suggest URL
        var suggestURL = "/SearchAutoCompleteCity";
        //Create Javascript object
        var searchData = { SelctedSearchPrefix: request.term, SelectedValue: selectedValue };

        $.ajax({
            url: suggestURL,
            data: JSON.stringify(searchData),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.length !== 0) {
                    response($.map(data, function (item) {
                        return { label: item.Value, value: item.Value, id: item.Value };
                    }));
                }
                else {
                    $("#DeliveryLocation").val("");
                }
            },
            error: function (response) {
                alert(response.responseText);
            },
            failure: function (response) {
                alert(response.responseText);
            }
        });
    },
    select: function (event, ui) {
        isDeliverySelected = true;
        $("#DeliveryLocation").val(ui.item.value);
    },
    change: function (event, ui) {
        if (!isDeliverySelected) {
            $("#PickupLocation").val("");
        }
    },
    close: function () {
        if (!isDeliverySelected) {
            $("#DeliveryLocation").val("");
        }
    }
}).data("autocomplete")._renderItem = function (ul, item) {
    return $("<li>")
        .data("item.autocomplete", item)
        .append("<a class='auto-complete'><i class='fa fa-building'></i> " + item.label + "<br />" + "</a>")
        .appendTo(ul);
};
}

 var isPickupSelected = false;
// Same guard as #DeliveryLocation above — this field lives in the same optional widget.
if ($("#PickupLocation").length) {
$("#PickupLocation").autocomplete({
    delay: 100,
    minLength: 3,
    source: function (request, response) {
        // Suggest URL
        var suggestURL = "/SearchAutoCompleteCity";
        //Create Javascript object
        var searchData = { SelctedSearchPrefix: request.term, SelectedValue: selectedValue };

        $.ajax({
            url: suggestURL,
            data: JSON.stringify(searchData),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (data.length !== 0) {
                    response($.map(data, function (item) {
                        return { label: item.Value, value: item.Value, id: item.Value };
                    }));
                }
                else {
                    $("#PickupLocation").val("");
                }
            },
            error: function (response) {
                alert(response.responseText);
            },
            failure: function (response) {
                alert(response.responseText);
            }
        });
    },
    select: function (event, ui) {
        isPickupSelected = true;
        $("#PickupLocation").val(ui.item.value);
    },
    change: function (event, ui) {
        if (!isPickupSelected) {
            $("#PickupLocation").val("");
        }
    },
    close: function () {
        if (!isPickupSelected) {
            $("#PickupLocation").val("");
        }
    }
}).data("autocomplete")._renderItem = function (ul, item) {
    return $("<li>")
        .data("item.autocomplete", item)
        .append("<a class='auto-complete'><i class='fa fa-building'></i> " + item.label + "<br />" + "</a>")
        .appendTo(ul);
};
}


})(this);