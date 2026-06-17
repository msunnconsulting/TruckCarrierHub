(function (global) {
    "use strict";

    //variable
    var vm = global.vm = {};
    var selectedValue = "";
    var cityStateNameInSearchTextbox = "";
    var cityName = "";
    var state = "";
    var filter = {};
    var url = $("#filterCompanyForm").attr("action");
    var LatLng = "";
    var boundSouthWestLatLng = "";
    var boundNorthEastLatLng = "";
    var boundNorthWestLatLng = "";
    var boundSouthEastLatLng = "";
    var boundryLatlongValues = "";
    var map;
    var markers = []; // Create a marker array to hold markers
    var isCityFound = true;
    var isCityChangedFromMapView = false;
    var zoomLevel = 11;
    var isRestrictResultToCity = false;
    var center;
    var bounds;
    //bound functions
    vm.OnPageChange = OnPageChange;
    vm.SearchFilterResult = SearchFilterResult;
    vm.OnSuccessSearchFilterResult = OnSuccessSearchFilterResult;
    vm.OnFailureSearchFilterResult = OnFailureSearchFilterResult;
    vm.MapView = MapView;       //when user click on map link button
    vm.MapToListView = MapToListView;   //when user click on list button

    init();
    function init() {

        //when user want to go back and forward after history.pushstate used
        window.onpopstate = function (event) {
            window.location.href = document.location.href;
        };

        $(".hide-list-map-toggle-column").hide();
        //get city name
        cityName = $("#cityName").val();
         
        state = $("#stateCode").val();
        //set state city name into search textbox
        if (state != null && state != '' && state != undefined && cityName != null && cityName != '' && cityName != undefined) {
            var setValue = cityName + ", " + state;
            $(".searchFilterTextbox").val(setValue);
        }
        //auto complete function
        $(".searchFilterTextbox").autocomplete({
            delay: 100,
            minLength: 3,
            source: function (request, response) {
                // Suggest URL
                var suggestURL = "/searchautocomplete";
                //Create Javascript object    
                var searchData = { SelctedSearchPrefix: request.term, SelectedValue: "City" };
                // JSONP Request
                $.ajax({
                    url: suggestURL,
                    data: JSON.stringify(searchData),
                    dataType: "json",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        $("#al").hide();
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
                    },
                    failure: function (response) {
                    }
                });
            }
            , select: function (event, ui) {
                //set values in search textbox
                $(".searchFilterTextbox").val(ui.item.value);
                SearchFilterResult()
            }
        });
        //set tags for filter values from url
        $('#tags').tagsInput({
            'height': '50px',
            'width': '1000px',
            'border': '0px',
            'interactive': false,
            'defaultText': '',
            'delimiter': [',', ';'],
            'onRemoveTag': function Remove(tag) { OnRemoveTag(tag); },
            'removeWithBackspace': false,
            'noDuplicates': true,
            'itemValue': 'value',
        });

        var windowUrl = window.location.pathname;
        //check if url contain position
        if (!windowUrl.includes('pos')) {
            $('.list-toggle-button').addClass('btn-primary');
        }
            //check if url is contain position values (if we enter url into other tab)
            //if it contains pos_lst then we have to display company list which is coming under this boundry values
        else if (windowUrl.includes('pos_lst')) {
            url = windowUrl;
            //get position values from window url
            var positionValues = window.location.pathname.match("pos_lst(.*)");
            if (positionValues[0].includes("/"))
            { positionValues = positionValues[0].split("/"); }
            positionValues = positionValues[0];
            positionValues = positionValues.split("pos_lst-")[1];
            filter['pos_lst'] = [positionValues];
            $('.list-toggle-button').addClass('btn-primary');
            $("#filterCompanyForm").attr("action", url);
            $("#filterCompanyForm").submit();
        }
        else {                       //else it contains only 'pos' then display map 
            isRestrictResultToCity = true;
            $('.map-toggle-button').addClass('btn-primary');
            //get boundry values from url
            var positionValues = window.location.pathname.match("pos(.*)");
            if (positionValues[0].includes("/"))
            { positionValues = positionValues[0].split("/"); }
            positionValues = positionValues[0];
            var allValues = positionValues.split(",");
            if (allValues.length > 4) {
                zoomLevel = positionValues.split(",").pop(-1);
            }
            else { isRestrictResultToCity = false; }
            positionValues = positionValues.split("pos-")[1];
            //set values into filter
            filter['pos'] = [positionValues];
            //call ajaxrequest for map
            AjaxCallForMapView();
        }
    
        //set tags and checkbox from url
        SetTagsAndCheckboxFromUrl();
        //check if tags are available then show clear filter button
        CheckTagInputIsAvailable();


        //if filter value is null or its object is null then get filter value from local storage for apply filter 
        if (filter == undefined || filter == '' || filter == null || jQuery.isEmptyObject(filter)) {
            //Get filter from local storage .
            var getFilterFromLocalStorage = window.localStorage.getItem("filter");
            //Convert json string to object value
            filter = JSON.parse(getFilterFromLocalStorage);

            //After convert json string to object  then check filter value is not null or its object is not empty 
            if (filter != undefined && filter != '' && filter != null && !jQuery.isEmptyObject(filter)) {
                //if filter has value, and its contains position list and position is not same for other city so remove "pos_lst" and "pos" from filter array
                if ("pos_lst" in filter)
                { delete filter.pos_lst; }
                if ("pos" in filter)
                { delete filter.pos; }

                //then crate url from local storage and just submit form this will manage entire process.
                createURLFromLocalStorare()
            }
            else {
                //after convert json if filter value is undefined or empty then initialize filter and set into local storage.
                filter = {}
                var myJSON = JSON.stringify(filter);
                window.localStorage.setItem("filter", myJSON);
            }
        }
        else {
            var myJSON = JSON.stringify(filter);
            window.localStorage.setItem("filter", myJSON);

        }
    }

    //when user remove any filter tag
    function OnRemoveTag(tag) {
        //for filter on entity type
        //first check this is applied or not
        if ("entity" in filter) {
            //split values by ,
            var selectedEntityValues = filter.entity[0].split(",");
            var entityCheckboxList = $("[class*='entityType-checkbox']");
            entityCheckboxList.each(function (index) {
                var item = $(this);
                //var labelName = item[0].value;
                var labelName = $('label[for=' + item.prop('id') + ']').text();
                if (labelName == tag) {
                    //remove selected tag from total values
                    selectedEntityValues.splice($.inArray(item.val(), selectedEntityValues), 1);
                    item.attr('checked', false);
                }
            });
            //delete filter
            delete filter.entity;
            //join values with commasaperated
            selectedEntityValues = selectedEntityValues.join(",");
            //check if there is selected values are in there or not
            if (selectedEntityValues != "") {
                CreateUrl("entity", selectedEntityValues);
            }
                //if there is no selected values then Any name checkbox selected and remove this filter from url
            else {
                selectedEntityValues = "Any";
                filter['entity'] = [selectedEntityValues];
                CreateUrl('entity', selectedEntityValues);
                $('.entityType-checkbox').attr('checked', false);
                $("#entitytype-any").prop('checked', true);
            }
        }

        //for Cargo type filter
        if ("cargo" in filter) {
            //split values by ,
            var cargoSelectedvalues = filter.cargo[0].split(",");
            //remove selected tag from total values
            var cargoCheckboxListlist = $('input[name="SelectedCargoTypes"]');
            cargoCheckboxListlist.each(function (index) {
                var item = $(this);
                var labelName = $('label[for=' + item.prop('id') + ']').text();
                if (labelName == tag) {
                    cargoSelectedvalues.splice($.inArray(item.val(), cargoSelectedvalues), 1);
                    item.attr('checked', false);
                }
            });
            //delete filter
            delete filter.cargo;
            //join values with commasaperated
            cargoSelectedvalues = cargoSelectedvalues.join(",");
            //check if there is selected values are in there or not
            if (cargoSelectedvalues != "") {
                CreateUrl("cargo", cargoSelectedvalues);
            }
                //if there is no selected values then All name checkbox selected and remove this filter from url
            else {
                cargoSelectedvalues = "All";
                filter['cargo'] = [cargoSelectedvalues];
                CreateUrl('cargo', cargoSelectedvalues);
                $('input[name="SelectedCargoTypes"]').attr('checked', false);
                $("#cargotype-all").prop('checked', true);
            }
        }

        //for service type filter
        if ("service" in filter) {
            //split values by ,
            var serviceSelectedvalues = filter.service[0].split(",");
            //remove selected tag from total values
            var serviceCheckboxListlist = $('input[name="SelectedServiceTypes"]');
            serviceCheckboxListlist.each(function (index) {
                var item = $(this);
                var labelName = $('label[for=' + item.prop('id') + ']').text();
                if (labelName == tag) {
                    serviceSelectedvalues.splice($.inArray(item.val(), serviceSelectedvalues), 1);
                    item.attr('checked', false);
                }
            });
            //delete filter
            delete filter.service;
            //join values with commasaperated
            serviceSelectedvalues = serviceSelectedvalues.join(",");
            //check if there is selected values are in there or not
            if (serviceSelectedvalues != "") {
                CreateUrl("service", serviceSelectedvalues);
            }
                //if there is no selected values then All name checkbox selected and remove this filter from url
            else {
                serviceSelectedvalues = "All";
                filter['cargo'] = [serviceSelectedvalues];
                CreateUrl('cargo', serviceSelectedvalues);
                $('input[name="SelectedServiceTypes"]').attr('checked', false);
                $("#servictype-all").attr('checked', true);
            }
        }

        //for trailer type filter
        if ("trailer" in filter) {
            //split values by ,
            var trailerSelectedvalues = filter.trailer[0].split(",");
            //remove selected tag from total values
            var trailerCheckboxListlist = $('input[name="SelectedTrailerTypes"]');
            trailerCheckboxListlist.each(function (index) {
                var item = $(this);
                var labelName = $('label[for=' + item.prop('id') + ']').text();
                if (labelName == tag) {
                    trailerSelectedvalues.splice($.inArray(item.val(), trailerSelectedvalues), 1);
                    item.attr('checked', false);
                }
            });
            //delete filter
            delete filter.trailer;
            //join values with commasaperated
            trailerSelectedvalues = trailerSelectedvalues.join(",");
            //check if there is selected values are in there or not
            if (trailerSelectedvalues != "") {
                CreateUrl("trailer", trailerSelectedvalues);
            }
            //if there is no selected values then All name checkbox selected and remove this filter from url
            else {
                trailerSelectedvalues = "Not Interested";
                filter['trailer'] = [trailerSelectedvalues];
                CreateUrl('trailer', trailerSelectedvalues);
                $('input[name="SelectedTrailerTypes"]').attr('checked', false);
                $("#trailertype-all").attr('checked', true);
            }
        }

        //for driver type filter
        if ("driver" in filter) {
            //split values by ,
            var driverSelectedvalues = filter.driver[0].split(",");
            //remove selected tag from total values
            var driverCheckboxListlist = $('input[name="SelectedDriverTypes"]');
            driverCheckboxListlist.each(function (index) {
                var item = $(this);
                var labelName = $('label[for=' + item.prop('id') + ']').text();
                if (labelName == tag) {
                    driverSelectedvalues.splice($.inArray(item.val(), driverSelectedvalues), 1);
                    item.attr('checked', false);
                }
            });
            //delete filter
            delete filter.driver;
            //join values with commasaperated
            driverSelectedvalues = driverSelectedvalues.join(",");
            //check if there is selected values are in there or not
            if (driverSelectedvalues != "") {
                CreateUrl("driver", driverSelectedvalues);
            }
            //if there is no selected values then All name checkbox selected and remove this filter from url
            else {
                driverSelectedvalues = "Not Interested";
                filter['driver'] = [driverSelectedvalues];
                CreateUrl('driver', driverSelectedvalues);
                $('input[name="SelectedDriverTypes"]').attr('checked', false);
                $("#drivertype-all").attr('checked', true);
            }
        }

        //for truckortractor filter values
        if ("truckortractor" in filter && filter.truckortractor[0].includes(tag)) {
            //split values by ,
            var truckortractorSelectedvalues = filter.truckortractor[0].split(",");
            //remove selected tag from total values
            var truckortractorCheckboxList = $("input[name='SelectedTruckOrTractor']");
            truckortractorCheckboxList.each(function (index) {
                var item = $(this);
                var labelName = item[0].value;
                if (labelName == tag) {
                    truckortractorSelectedvalues.splice($.inArray(item.val(), truckortractorSelectedvalues), 1);
                    item.attr('checked', false);
                }
            });
            //delete filter
            delete filter.truckortractor;
            //join values with commasaperated
            truckortractorSelectedvalues = truckortractorSelectedvalues.join(",");
            //check if there is selected values are in there or not
            if (truckortractorSelectedvalues != "") {
                CreateUrl("truckortractor", truckortractorSelectedvalues);
            }
                //if there is no selected values then All name checkbox selected and remove this filter from url
            else {
                truckortractorSelectedvalues = "Any";
                filter['truckortractor'] = [truckortractorSelectedvalues];
                CreateUrl('truckortractor', truckortractorSelectedvalues);
                $('.trucktractor-checkbox').attr('checked', false)
                $("#trucktracktor-any").attr('checked', true);
            }
        }

        //for sort by filter values
        if ("sortby" in filter) {
            //split values by ,
            var sortbySelectedvalues = filter.sortby[0];

            //CreateUrl("sortby", "");
            var sortbyCheckboxListlist = $("input[name='SortBy']");
            sortbyCheckboxListlist.each(function (index) {
                var item = $(this);
                var checkBoxName = $('label[for=' + item.prop('id') + ']').text();;
                // sortbyValue = sortbySelectedvalues.replace("-", " ");
                //check if value is available or not
                if (checkBoxName == tag) {
                    CreateUrl("sortby", "");
                }

            });
        }

        //If All filter is cleared to identify there is no filter applied
        if (filter.length == undefined || filter.length == 0) {
            var myJSON = JSON.stringify(filter);
            window.localStorage.setItem("filter", myJSON);
        }
          

        $("#filterCompanyForm").submit();
    }


    function createURLFromLocalStorare() {
        SetFilterValuesInUrl()
        $("#filterCompanyForm").submit();
    }

    //create tags and checkboxes from Url
    function SetTagsAndCheckboxFromUrl() {
        $('#tags').importTags('');
        //check Cargotype filter
        //get value from url if cargoType filter applied
        var cargoValues = window.location.pathname.match("cargo-(.*)");
        //if cargoValues array is not empty
        if (cargoValues && cargoValues.length) {
            if (cargoValues[0].includes("/"))
            { cargoValues = cargoValues[0].split("/"); }
            cargoValues = cargoValues[0];
            cargoValues = cargoValues.split("cargo-")[1];
            //add values into filter
            filter["cargo"] = [cargoValues];
            //now check selected checkbox
            var cargoCheckboxListlist = $('input[name="SelectedCargoTypes"]');
            cargoValues = cargoValues.split(',');
            cargoCheckboxListlist.each(function (index) {
                var item = $(this);
                if ($.inArray(item.val(), cargoValues) != -1) {
                    
                    $(this).prop('checked', true);
                    $('#tags').addTag($('label[for=' + item.prop('id') + ']').text());
                    $("#cargotype-all").attr('checked', false);
                } else {
                }
            });
        }
        else {
            //uncheck all selected checkboxes
            $('input[name="SelectedCargoTypes"]').attr('checked', false);
            $("#cargotype-all").attr('checked', true);
        }

        //check Entity type filter
        //get value from url if entity filter applied
        var entityValues = window.location.pathname.match("entity-(.*)");
        //check if entity values are selected
        if (entityValues && entityValues.length) {
            if (entityValues[0].includes("/")) {
                entityValues = entityValues[0].split("/");
            }
            entityValues = entityValues[0];
            entityValues = entityValues.split("entity-")[1];
            filter["entity"] = [entityValues];
            //now check selected checkbox
            var entityCheckboxList = $("[class*='entityType-checkbox']");
            entityCheckboxList.each(function (index) {
                var item = $(this);
                entityValues = entityValues.replace("-", " ");
                //check if value is available or not
                if (entityValues.indexOf(item.val()) != -1) {
                    $(this).prop('checked', true);
                    //now set tag of this entity name
                    $('#tags').addTag($('label[for=' + item.prop('id') + ']').text());
                    $("#entitytype-any").attr('checked', false);
                }
            });
        }
        else {
            $('.entityType-checkbox').attr('checked', false);
            $("#entitytype-any").prop('checked', true);
        }

        //check Service type filter
        //get value from url if service filter applied
        var serviceValues = window.location.pathname.match("service-(.*)");
        if (serviceValues && serviceValues.length) {
            if (serviceValues[0].includes("/")) {
                serviceValues = serviceValues[0].split("/");
            }
            serviceValues = serviceValues[0];
            serviceValues = serviceValues.split("service-")[1];
            filter["service"] = [serviceValues];
            //now check selected checkbox
            var serviceCheckboxListlist = $("input[name='SelectedServiceTypes']");
            serviceValues = serviceValues.replace("-", " ");
            serviceValues = serviceValues.split(',');
            serviceCheckboxListlist.each(function (index) {
                var item = $(this);
                if ($.inArray(item.val(), serviceValues) != -1) {
                    $(this).prop('checked', true);
                    $('#tags').addTag($('label[for=' + item.prop('id') + ']').text());
                    $("#servictype-all").attr('checked', false);
                } else {
                }
            });
        }
        else {
            $('input[name="SelectedServiceTypes"]').attr('checked', false);
            $("#servictype-all").prop('checked', true);
        }

        //check Trailer type filter
        //get value from url if trailer filter applied
        var trailerValues = window.location.pathname.match("trailer-(.*)");
        if (trailerValues && trailerValues.length) {
            if (trailerValues[0].includes("/")) {
                trailerValues = trailerValues[0].split("/");
            }
            trailerValues = trailerValues[0];
            trailerValues = trailerValues.split("trailer-")[1];
            filter["trailer"] = [trailerValues];
            //now check selected checkbox
            var trailerCheckboxListlist = $("input[name='SelectedTrailerTypes']");
            trailerValues = trailerValues.replace("-", " ");
            trailerValues = trailerValues.split(',');
            trailerCheckboxListlist.each(function (index) {
                var item = $(this);
                if ($.inArray(item.val(), trailerValues) != -1) {
                    $(this).prop('checked', true);
                    $('#tags').addTag($('label[for=' + item.prop('id') + ']').text());
                    $("#trailertype-all").attr('checked', false);
                } else {
                }
            });
        }
        else {
            $('input[name="SelectedTrailerTypes"]').attr('checked', false);
            $("#trailertype-all").prop('checked', true);
        }

        //check Driver type filter
        //get value from url if trailer filter applied
        var driverValues = window.location.pathname.match("driver-(.*)");
        if (driverValues && driverValues.length) {
            if (driverValues[0].includes("/")) {
                driverValues = driverValues[0].split("/");
            }
            driverValues = driverValues[0];
            driverValues = driverValues.split("driver-")[1];
            filter["driver"] = [driverValues];
            //now check selected checkbox
            var driverCheckboxListlist = $("input[name='SelectedDriverTypes']");
            driverValues = driverValues.replace("-", " ");
            driverValues = driverValues.split(',');
            driverCheckboxListlist.each(function (index) {
                var item = $(this);
                if ($.inArray(item.val(), driverValues) != -1) {
                    $(this).prop('checked', true);
                    $('#tags').addTag($('label[for=' + item.prop('id') + ']').text());
                    $("#drivertype-all").attr('checked', false);
                } else {
                }
            });
        }
        else {
            $('input[name="SelectedDriverTypes"]').attr('checked', false);
            $("#drivertype-all").prop('checked', true);
        }

        //check truck or tractor  filter
        //get value from url if truck or tractor filter applied
        var tracktractorValues = window.location.pathname.match("truckortractor-(.*)");
        if (tracktractorValues && tracktractorValues.length) {

            if (tracktractorValues[0].includes("/"))
            { tracktractorValues = tracktractorValues[0].split("/"); }
            tracktractorValues = tracktractorValues[0];
            tracktractorValues = tracktractorValues.split("truckortractor-")[1];
            filter["truckortractor"] = [tracktractorValues];
            //now check selected checkbox
            var tracktractoCheckboxList = $("input[name='SelectedTruckOrTractor']");
            tracktractorValues = tracktractorValues.split(',');
            tracktractoCheckboxList.each(function (index) {
                var item = $(this);
                if ($.inArray(item.val(), tracktractorValues) != -1) {
                    $(this).prop('checked', true);
                    $('#tags').addTag(item.val());
                    $("#trucktracktor-any").attr('checked', false);
                }
            });
        }
        else {
            $('.trucktractor-checkbox').attr('checked', false);
            $("#trucktracktor-any").prop('checked', true);
        }

        //for sort by values
        var sortbyValue = window.location.pathname.match("sortby-(.*)");
        if (sortbyValue && sortbyValue.length) {
            if (sortbyValue[0].includes("/")) {
                sortbyValue = sortbyValue[0].split("/");
            }
            sortbyValue = sortbyValue[0];
            sortbyValue = sortbyValue.split("sortby-")[1];
            filter["sortby"] = [sortbyValue];
            var sortbyCheckboxListlist = $("input[name='SortBy']");
            sortbyCheckboxListlist.each(function (index) {
                var item = $(this);
                var checkBoxName = $('label[for=' + item.prop('id') + ']').text();
                //var checkBoxName = item[0].labels[0].innerHTML;
                sortbyValue = sortbyValue.replace("-", " ");
                //check if value is available or not
                if (sortbyValue.indexOf(item.val()) != -1) {
                    $(this).prop('checked', true);
                    $('#tags').addTag(checkBoxName);
                }
            });
        }
        else { $('input[id="Relevance"]').prop('checked', true); }
    }

    //search filter result
    //this will call when user search from the text box
    function SearchFilterResult() {
        //get city name from search textbox
        cityStateNameInSearchTextbox = $(".searchFilterTextbox").val();
        //get browser window url
        var windowUrl = window.location.pathname;
        if (cityStateNameInSearchTextbox == "") {
            ShowDialogBox('Info', "No city found", 'Ok', '', '', null);
            SetCityStateValueInSearchTextbox();
        }
        else if (!isCityFound) {
            isCityFound = true;
            ShowDialogBox('Info', "No city found", 'Ok', '', '', null);
            SetCityStateValueInSearchTextbox();
        }
        else if (windowUrl.includes('pos') && $(".map-toggle-button").hasClass("btn-primary")) {
            var searchedCityName = cityStateNameInSearchTextbox.split(','); // for display city name in breadCrumb split city name and code.
            $("#anchorCityName").html(searchedCityName[0]);// display city name in breadCrumb
            isCityChangedFromMapView = true;
            zoomLevel = 10;
            AjaxCallForMapView("");
        }
        else {
            cityStateNameInSearchTextbox = cityStateNameInSearchTextbox.split(",");
            cityName = cityStateNameInSearchTextbox[0];
            state = cityStateNameInSearchTextbox[1];
            //remove position filter if available
            if ("pos_lst" in filter)
            { delete filter.pos_lst; }
            SetFilterValuesInUrl();
            $("#anchorCityName").html(cityName);
            $("#filterCompanyForm").submit();
        }
    }

    //if user enter key for search
    $('.searchFilterTextbox').on('keypress', function (e) {
        if (e.which == 13 && $(this).val()) {
            SearchFilterResult();
        }
    });

    //var newUrl = url;
    //create url by filter
    //check if filter values are ="" or any then delete this filter
    function CreateUrl(filterName, value) {
        if (filterName in filter && filterName == "entity" && (value == "" || value == "Any")) {
            delete filter.entity;
        }
        else if (filterName in filter && filterName == "cargo" && (value == "" || value == "All")) {
            delete filter.cargo;
        }
        else if (filterName in filter && filterName == "service" && (value == "" || value == "All")) {
            delete filter.service;

        }
        else if (filterName in filter && filterName == "trailer" && (value == "" || value == "Not Interested")) {
            delete filter.trailer;
        }
        else if (filterName in filter && filterName == "driver" && (value == "" || value == "Not Interested")) {
            delete filter.driver;
        }
        else if (filterName in filter && filterName == "truckortractor" && (value == "" || value == "Any")) {
            delete filter.truckortractor;
        }
        else if (filterName in filter && filterName == "sortby" && (value == "")) {
            delete filter.sortby;
        }
        else {
            //create property with array values
            filter[filterName] = [value];
        }
        //set filter values in url
        SetFilterValuesInUrl();
    }

    //get each filter values and create url
    function SetFilterValuesInUrl() {
        var filterValues = "";
        $.each(filter, function (key, values) {
            filterValues += key + "-" + values + "/";
        });
        var citynameurl = "";
        //create url
        if (cityName.includes(' ')) {
            citynameurl = cityName.split(" ").join("-");
        }
        else {
            citynameurl = cityName;
        }
        
        url = "/" + state.trim() + "/" + citynameurl + "/" + filterValues;
        //set url into form
        $("#filterCompanyForm").attr("action", url);
    }

    //check if is there any inputag is available 
    //if available then display clear filter button
    function CheckTagInputIsAvailable() {
        var tags = [];
        $.map($(".tagsinput span span"), function (e, i) {
            tags.push($(e).text());
        });
        //if there is filter tag availale then show clear filter button else hide 
        if (tags.length <= 0) {
            $("#span-tag").hide();
        }
        else {
            $("#span-tag").show();
        }
    }

    //success call of filter result
    //we have to change url after successfully get the filtered result
    function OnSuccessSearchFilterResult(response) {
        isCityFound = true;
        var windowUrl = window.location.pathname;
        $(".cityName-in-map").text(response.hdnCityName);
        $(".cityNameText").text(response.hdnCityName);
        if ("pos" in filter) {
            response.NewUrl = "/" + response.NewUrl;
            window.history.pushState("Details", "Title", response.NewUrl);
            //load map and set markers on filtered companies
            LoadMapAfterFilterSuccessfullyApplied(response.Companies.Items, response.Companies.Pagination.TotalCount);
        }
        else {
            window.history.pushState("Details", "Title", $("#filterCompanyForm").attr("action"));
            //load map after successfullyy applied filter
            $('#companyList').html(response.AjaxReturn);
            $(".total-companies").text(response.TotalCompanies);
            //$(".pageDescription-cityPage").text(response.companyVM.PageDescription);
            $(".stateName").text(response.companyVM.StateName)
            $("a.stateName").attr("href", "/" + response.companyVM.StateCode);
            document.title = response.companyVM.PageTitle;
        }
        //set input tags and checkbox
        SetTagsAndCheckboxFromUrl();
        //check any tag is available if not then hide clear filter button
        CheckTagInputIsAvailable();
    }

    //in map if user apply filter then load map after getting company information
    function LoadMapAfterFilterSuccessfullyApplied(data, totalCompaniesFound) {
        //get city name from search textbox
        cityStateNameInSearchTextbox = $(".searchFilterTextbox").val();
        //display total companies found result
        $(".company-result-in-map").text(totalCompaniesFound);
        $(".total-companies").text(totalCompaniesFound);
        //get url action value      
        cityStateNameInSearchTextbox = cityStateNameInSearchTextbox.split(",");
        cityName = cityStateNameInSearchTextbox[0];
        state = cityStateNameInSearchTextbox[1];
        var fullAddress = cityName + state;
        //get center latlng values
        center = map.getCenter();
        //set center latlng
        var centerLatlng = { lat: center.lat(), lng: center.lng() };
        // Loop through markers and set map to null for each
        for (var i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
        // Reset the markers array
        markers = [];
        //redraw  markers
        SetMarkers(map, data, state, cityName);
    }

    function OnFailureSearchFilterResult() { }

    ///if user apply any filter and change page
    function OnPageChange(oldurl) {
        //split old url and get pagination values from that
        var urlvalues = oldurl.split('?');
        //get browser window url
        var windowUrl = window.location.pathname;
        var url = windowUrl;
        //set pagination values
        url += "?" + urlvalues[1];
        //set url to form 
        $("#filterCompanyForm").attr('action', url);
        //submit form
        $("#filterCompanyForm").submit();
    }

    //for entity type checkbox list filter
    $('input[name="SelectedEntityTypes"]').click(function () {
        if ($("#entitytype-any").prop('checked') == true) {
            $("#entitytype-any").attr('checked', false);
        }
        SetCityStateValueInSearchTextbox();
    });

    //if user check any in entitytype checkbox then uncheck all
    $('input[id="entitytype-any"]').click(function () {
        if ($("#entitytype-any").prop('checked') == true) {
            $('.entityType-checkbox').attr('checked', false)
            var currentcheckboxvalue = $(this).val();
        }
    });

    //for cargotype checkbox list filter
    $('input[name="SelectedCargoTypes"]').click(function () {
        if ($("#cargotype-all").prop('checked') == true) {
            $("#cargotype-all").attr('checked', false);
        }
        SetCityStateValueInSearchTextbox();
    });

    //if user check All in cargotype then uncheck other checkbox
    $('input[id="cargotype-all"]').click(function () {
        if ($("#cargotype-all").prop('checked') == true) {
            $('input[name="SelectedCargoTypes"]').attr('checked', false);
            var currentcheckboxvalue = $(this).val();
        }
    });

    //for selected truckortractor checkbox list filter
    $('input[name="SelectedTruckOrTractor"]').click(function () {
        if ($("#trucktracktor-any").prop('checked') == true) {
            $("#trucktracktor-any").attr('checked', false);
        }
        SetCityStateValueInSearchTextbox();
    });

    //if user check any in trucktracktor then uncheck other checkbox
    $('input[id="trucktracktor-any"]').click(function () {
        if ($("#trucktracktor-any").prop('checked') == true) {
            $('.trucktractor-checkbox').attr('checked', false)
            var currentcheckboxvalue = $(this).val();
        }
    });

    //for selected service types
    $('input[name="SelectedServiceTypes"]').click(function () {
        if ($("#servictype-all").prop('checked') == true) {
            $("#servictype-all").attr('checked', false);
        }
        SetCityStateValueInSearchTextbox();
    });

    //if user check All in servicetype then uncheck other checkbox
    $('input[id="servictype-all"]').click(function () {
        if ($("#servictype-all").prop('checked') == true) {
            $('input[name="SelectedServiceTypes"]').attr('checked', false)
            var currentcheckboxvalue = $(this).val();
        }
    });

    //for selected Trailer types
    $('input[name="SelectedTrailerTypes"]').click(function () {
        if ($("#trailertype-all").prop('checked') == true) {
            $("#trailertype-all").attr('checked', false);
        }
        SetCityStateValueInSearchTextbox();
    });

    //if user check All in Trailer type then uncheck other checkbox
    $('input[id="trailertype-all"]').click(function () {
        if ($("#trailertype-all").prop('checked') == true) {
            $('input[name="SelectedTrailerTypes"]').attr('checked', false);
            var currentcheckboxvalue = $(this).val();
        }
    });

    //for selected Driver types
    $('input[name="SelectedDriverTypes"]').click(function () {
        if ($("#drivertype-all").prop('checked') == true) {
            $("#drivertype-all").attr('checked', false);
        }
        SetCityStateValueInSearchTextbox();
    });

    //if user check All in Driver type then uncheck other checkbox
    $('input[id="drivertype-all"]').click(function () {
        if ($("#drivertype-all").prop('checked') == true) {
            $('input[name="SelectedDriverTypes"]').attr('checked', false);
            var currentcheckboxvalue = $(this).val();
        }
    });

    //clear all tags
    $("#clearTags").on('click', function () {
        //clear all filter on click this button
        //first check which filter is available
        if ('entity' in filter) {
            delete filter.entity;
        }
        if ('cargo' in filter) {
            delete filter.cargo;
        }
        if ('service' in filter) {
            delete filter.service;
        }
        if ('trailer' in filter) {
            delete filter.trailer;
        }
        if ('driver' in filter) {
            delete filter.driver;
        }
        if ('sortby' in filter) {
            delete filter.sortby;
        }
        if ('truckortractor' in filter) {
            delete filter.truckortractor;
        }
        SetFilterValuesInUrl();
      

        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);

        $("#filterCompanyForm").submit();
    });

    /////////////////////////////////////////// For Map View //////////////////////////////////
    //when user click on map button then this function will be called
    function MapView() {
        
        isRestrictResultToCity = false;
        //check if list already shown in browser window
        var isMapAlreadyShown = $(".map-toggle-button").hasClass("btn-primary");    //if return false then display map else does not send any request
        if (!isMapAlreadyShown) {
            //hide company list
            $("#companyList").hide();
            $(".sort-by-option").hide();
            //add class for displaying active button
            $('.map-toggle-button').addClass('btn-primary');
            //remove class from list button
            $('.list-toggle-button').removeClass('btn-primary');
            //display map
            $("#mapView").show();
            //check if pos list filter is available or not
            //if available then delete it first
            if ("pos_lst" in filter) {
                delete filter.pos_lst;
            }
            //get browser window url
            var windowUrl = window.location.pathname;
            //get form url
            url = $("#filterCompanyForm").attr("action");
            //if user first check map after that check list and then check again map
            if (windowUrl.includes('pos_lst')) {
                url = windowUrl;
                url = url.replace("pos_lst", "pos");    //change url set pos to pos_lst
                //set new created url
                $("#filterCompanyForm").attr("action", url);
                //when boundry values ="" then get position values from windowurl and add into filter
                var positionValues = window.location.pathname.match("pos(.*)");
                //check if any other filter is already applied or not
                if (positionValues[0].includes("/"))
                { positionValues = positionValues[0].split("/"); }
                positionValues = positionValues[0];
                //get position values
                positionValues = positionValues.split("pos_lst-")[1];
                boundryLatlongValues = positionValues;
                var allValues = positionValues.split(",");
                if (allValues.length > 4) {
                    zoomLevel = positionValues.split(",").pop(-1);
                }
                filter["pos"] = [positionValues];
                if (map == undefined) {
                   var geocoder = new google.maps.Geocoder();
                    var centerlat = (parseFloat(allValues[0]) + parseFloat(allValues[2])) / 2;
                    var centerlng = (parseFloat(allValues[1]) + parseFloat(allValues[3])) / 2;
                    //set center latlng
                    center = { lat: centerlat, lng: centerlng };
                    cityStateNameInSearchTextbox = $(".searchFilterTextbox").val();
                    //get url action value      
                    cityStateNameInSearchTextbox = cityStateNameInSearchTextbox.split(",");
                    //get city name 
                    cityName = cityStateNameInSearchTextbox[0];
                    //get state name
                    state = cityStateNameInSearchTextbox[1];
                    var fullAddress = cityName + "," + state;
                    geocoder.geocode({ 'address': fullAddress }, function (results, status) {
                        if (status == google.maps.GeocoderStatus.OK) {
                            map = new google.maps.Map(document.getElementById('map'), {
                                center: center,
                                zoom: parseFloat(zoomLevel),
                                mapTypeId: google.maps.MapTypeId.ROADMAP,
                                zoomControl: true,
                                panControl: true,
                            });
                            //set markers on selected companies
                            google.maps.event.addListener(map, 'dragend', getPinsToMapBound);
                            google.maps.event.addListener(map, 'zoom_changed', getPinsToMapBound);
                        }
                    });
                }
                //submit form
                $("#filterCompanyForm").submit();
            }
            else {
                var myMarkers = [];
                //get city name from search textbox
                cityStateNameInSearchTextbox = $(".searchFilterTextbox").val();
                //get url action value      
                cityStateNameInSearchTextbox = cityStateNameInSearchTextbox.split(",");
                //get city name
                cityName = cityStateNameInSearchTextbox[0];
                //get state name
                state = cityStateNameInSearchTextbox[1];
                //set full address
                var fullAddress = cityName + "," + state;
                //replace space to '-' from city name 
                cityName = cityName.replace(" ", "-");
                var geocoder = new google.maps.Geocoder();
                //ajax request for display map into browser window
                AjaxCallForMapView(url);
            }
        }
    }

    //from this method we are getting first boundry values and set it into url
    //return this url
    function GetUrlAndBoundryValues(results) {
        LatLng = { lat: results[0].geometry.location.lat(), lng: results[0].geometry.location.lng() };
        //for southwest call getSouthWest() function and get latlng
        if (!results[0].geometry.bounds) {
            boundSouthWestLatLng = { lat: results[0].geometry.viewport.getSouthWest().lat(), lng: results[0].geometry.viewport.getSouthWest().lng() };
            //for northeast call getNorthEast() function and get latlng
            boundNorthEastLatLng = { lat: results[0].geometry.viewport.getNorthEast().lat(), lng: results[0].geometry.viewport.getNorthEast().lng() };
        }
        else {
            boundSouthWestLatLng = { lat: results[0].geometry.bounds.getSouthWest().lat(), lng: results[0].geometry.bounds.getSouthWest().lng() };
            //for northeast call getNorthEast() function and get latlng
            boundNorthEastLatLng = { lat: results[0].geometry.bounds.getNorthEast().lat(), lng: results[0].geometry.bounds.getNorthEast().lng() };
        }
        //set values and create as a string
        boundryLatlongValues = boundSouthWestLatLng.lat + "," + boundSouthWestLatLng.lng + "," +
           boundNorthEastLatLng.lat + "," + boundNorthEastLatLng.lng;
        //add position values into filter
        filter["pos"] = [boundryLatlongValues];
        //create url for filter
        CreateUrl("pos", boundryLatlongValues);
        //set url into form and return it
        return url = $("#filterCompanyForm").attr("action");
    }

    //common function for load map view
    function AjaxCallForMapView(url) {
        $(".sort-by-option").hide();
        //get browser window url
        var windowUrl = window.location.pathname;
        //get city name from search textbox
        cityStateNameInSearchTextbox = $(".searchFilterTextbox").val();
        //get url action value      
        cityStateNameInSearchTextbox = cityStateNameInSearchTextbox.split(",");
        //get city name 
        cityName = cityStateNameInSearchTextbox[0];
        //get state name
        state = cityStateNameInSearchTextbox[1];
        var fullAddress = cityName + "," + state;
        //create geocoder object
        var geocoder = new google.maps.Geocoder();
         
        $("#companyList").hide();
        $("#mapView").show();
        geocoder.geocode({ 'address': fullAddress }, function (results, status) {
            //when user click on map link button then first we will have to get its boundry values
            //check if window url is not containg any boundry values then
            if ((!windowUrl.includes('pos') && $("#mapView").click) || windowUrl.includes('pos_lst')) {
                url = GetUrlAndBoundryValues(results);
            }
                //if user chamge city name from search textbox in mapview then
            else if (windowUrl.includes('pos') && isCityChangedFromMapView) {
                url = GetUrlAndBoundryValues(results);
            }
                //direct enter url from other tab
            else {
                url = windowUrl;
            }
         
            $("#filterCompanyForm").attr("action", url);     //set url into form action,(this will be user after success ajax call)
            $.ajax({
                url: url,
                data: { isRestrictResultToCity: isRestrictResultToCity },
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    //$(".pageDescription-cityPage").text(response.PageDescription);
                    document.title = response.PageTitle;
                    $(".stateName").text(response.StateName)
                    $("a.stateName").attr("href", "/" + response.StateCode);
                    $(".hide-list-map-toggle-column").hide();
                    $(".found-result-companies-row").show();
                    ////if user enter url in other tab then  we have to get boundry values from url to find center point for map
                    var positionValues = window.location.pathname.match("pos(.*)");
                    //if only one company found then set it as center into map
                    if (isRestrictResultToCity == false && response.Companies.Items.length == 1)
                    {
                        center = { lat: response.Companies.Items[0].Latitude, lng: response.Companies.Items[0].Longitude};
                    }
                    else if (positionValues != null && isCityChangedFromMapView == false) {
                        if (positionValues[0].includes("/"))
                        { positionValues = positionValues[0].split("/"); }
                        positionValues = positionValues[0];
                        positionValues = positionValues.split("pos-")[1];
                        positionValues = positionValues.split(",");
                        boundryLatlongValues = positionValues;
                        //after getting boundry values we will have to find out center latlng from this
                        //here position values = swlat+swlng+nelat+nelng;
                        //from this we can get its center point lat lng
                        var centerlat = (parseFloat(positionValues[0]) + parseFloat(positionValues[2])) / 2;
                        var centerlng = (parseFloat(positionValues[1]) + parseFloat(positionValues[3])) / 2;
                        //set center latlng
                        center = { lat: centerlat, lng: centerlng };
                    } else {
                        //if user has click on map link button then directly it will show selected city as a center point
                        center = { lat: results[0].geometry.location.lat(), lng: results[0].geometry.location.lng() };
                    }
                    //get companies from coming response
                    var data = response.Companies.Items;
                    //if google api geocode is successfully getting details from its address then display map
                    if (status == google.maps.GeocoderStatus.OK) {
                        map = new google.maps.Map(document.getElementById('map'), {
                            center: center,
                            zoom: parseFloat(zoomLevel),
                            mapTypeId: google.maps.MapTypeId.ROADMAP
                        });
                        //callback event for zoom in/out or drag
                        //when user zoom or drag map at anywhere then redraw map boundries and display companies which are under this values
                        google.maps.event.addListener(map, 'dragend', getPinsToMapBound);
                        google.maps.event.addListener(map, 'zoom_changed', getPinsToMapBound);
                        //set markers on selected companies
                        SetMarkers(map, data, state, cityName);
                        $(".cityName-in-map").text(response.hdnCityName);
                        $(".cityNameText").text(response.hdnCityName);
                        $(".total-companies").text(response.Companies.Pagination.TotalCount);
                        $(".company-result-in-map").text(response.Companies.Pagination.TotalCount);
                        //after data recieved successfully set url in browser window.
                        window.history.pushState("Details", "Title", $("#filterCompanyForm").attr("action"));
                    }
                        //if geocode request failure to getting response from full address then display error message in alertboox
                    else {
                        alert('Geocode was not successful for the following reason: ' + status);
                    }
                },
                error: function (response) {
                },
                failure: function (response) {
                }
            });
        });
    }

    //when user drag or zoom then this function will be called
    //first get the boundry values and center
    //create and set url
    //call ajax request
    function getPinsToMapBound(ev) {
        //get bounds of map
        bounds = map.getBounds();
        //get center point
        center = map.getCenter();
        zoomLevel = map.getZoom();
        if (map.getZoom() < 8) {
            zoomLevel = 8;
            map.setZoom(zoomLevel);
        }
        if (map.getZoom() > 21) {
            zoomLevel = 21;
            map.setZoom(zoomLevel);
        }
        //set center latlng from enter point
        var centerLatlng = { lat: center.lat(), lng: center.lng() };
        //get boundryvalues
        boundNorthEastLatLng = { lat: bounds.getNorthEast().lat(), lng: bounds.getNorthEast().lng() };
        boundSouthWestLatLng = { lat: bounds.getSouthWest().lat(), lng: bounds.getSouthWest().lng() };
        boundryLatlongValues = boundSouthWestLatLng.lat + "," + boundSouthWestLatLng.lng + "," +
          boundNorthEastLatLng.lat + "," + boundNorthEastLatLng.lng + "," + zoomLevel;
        //set into our filter array
        filter["pos"] = [boundryLatlongValues];
        //set url
        CreateUrl("pos", boundryLatlongValues);
        //now call ajax request which has this url and center points
        GetCompaniesAfterZoomOrDrag(centerLatlng);
    }

    //after zoom in/out or drag call ajax request and get values
    function GetCompaniesAfterZoomOrDrag(centerLatLng) {
        //get Url
        url = $("#filterCompanyForm").attr("action");
        //call ajax request
        $.ajax({
            url: url,
            data: "",
            dataType: "json",
            // type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                zoomLevel = map.getZoom();
                var data = response.Companies.Items;
                //set all companies into map
                SetMarkers(map, data, state, cityName);
                $(".hide-list-map-toggle-column").hide();
                $(".total-companies").text(response.Companies.Pagination.TotalCount);
                //change url after successfully display map
                window.history.pushState("Details", "Title", $("#filterCompanyForm").attr("action"));
            },
            error: function (response) {
            },
            failure: function (response) {
            }
        });
    }

    //set marker on selected companies and display infowindow 
    //infowindow display company name 
    function SetMarkers(map, data, state, cityName) {
        var titleOnMarker = "";
        for (var i = 0; i < data.length; i++) {
            if (data[i].DoingBusinessAsName == null) {
                titleOnMarker = data[i].LegalName;
            }
            else { titleOnMarker = data[i].DoingBusinessAsName; }
            if (data[i].OfficeTelephoneNumber != null && data[i].OfficeTelephoneNumber != undefined) {
                data[i].OfficeTelephoneNumber = data[i].OfficeTelephoneNumber.replace(/(\d{3})(\d{3})(\d{4})/, "$1-$2-$3");
            }

            var markerLatLng = { lat: parseFloat(data[i].Latitude), lng: parseFloat(data[i].Longitude) };

            var marker = new google.maps.Marker({
                position: markerLatLng,
                map: map,
                title: titleOnMarker
            });
            //Url for displaying company information page
            titleOnMarker = titleOnMarker.replace('.', ''); // Arkady Sep 15 2019 
            var companyUrlRoute = "/" + state.trim() + "/USDOT-" + data[i].USDOTNumber;
            //replace '-' where as ' ' available
            companyUrlRoute = companyUrlRoute.split(' ').join('-');
            companyUrlRoute = companyUrlRoute.replace(/\'/g, "");    //if company name contains Affostrofy s like "'s" 
            companyUrlRoute = companyUrlRoute.replace(/\+/g, '-'); // Arkady 
            companyUrlRoute = companyUrlRoute.replace('&', '-'); // Arkady
                       
            if (data[i].OfficeTelephoneNumber == null) {
                data[i].OfficeTelephoneNumber = "N/A";
            }
            if (data[i].DoingBusinessAsName == null) {
                data[i].DoingBusinessAsName = "";
            }
            else { data[i].DoingBusinessAsName = data[i].DoingBusinessAsName; }
            //set content in infowindow
            var content = "<div style = 'height:90px;width:250px;'><b><u>Your location:</u></b><br /><b>Company Name:</b><a target='_blank' href='" + companyUrlRoute + "'>" + data[i].DoingBusinessAsName + "(" + data[i].LegalName + ")</a><br /><b>Phone:</b> <a href='tel:+1-" + data[i].OfficeTelephoneNumber + "'>" + data[i].OfficeTelephoneNumber + "</a>";
            // Display inactive red text for inactive companies
            if (data[i].Status && data[i].Status.toLowerCase() === "i") {
                content += "<br /><b style='color: #FF0000'>Inactive</b>";
            }
            var currentinfowindow = null;
            var infowindow = new google.maps.InfoWindow({ maxWidth: 320 });
            google.maps.event.addListener(marker, 'click', (function (marker, content, infowindow) {
                return function () {
                    //for at a time only one infowindow is open
                    if (currentinfowindow != null)      //check if any infowindow is open after click on infowindow
                    {
                        currentinfowindow.close(map, marker);      //close infowindow
                    }
                    infowindow.setContent(content);     //set content in infowindow
                    infowindow.open(map, marker);
                    currentinfowindow = infowindow;
                };
            })(marker, content, infowindow)
            );
            // Push marker to markers array
            markers.push(marker);
            //now click on anywhere in map it will close infowindow if it is open
            google.maps.event.addListener(map, "click", function (event) {
                //close all infowindow if i click anywhere in map
                if (currentinfowindow != null) {
                    currentinfowindow.close(map, marker);
                }
            });
        }
    }

    //when user click on list button from map then this function will called
    function MapToListView() {
        $(".hide-list-map-toggle-column").hide();
        isCityChangedFromMapView = false;
        isCityFound = true;
        //check if list already shown in browser window
        var isListActive = $(".list-toggle-button").hasClass("btn-primary");     //if isListActive return false then it will display list of companies
        if (!isListActive) {
            $("#companyList").show();
            $('.map-toggle-button').removeClass('btn-primary');
            $('.list-toggle-button').addClass('btn-primary');
            $("#mapView").hide();
            $(".sort-by-option").show();
            //get url from form action
            url = $("#filterCompanyForm").attr("action").replace("pos", "pos_lst");

            //set url for position list
            $("#filterCompanyForm").attr("action", url);
            //if position values are already available into filter then remove it first
            if ("pos" in filter) {
                delete filter.pos;
            }
            //when boundry values ="" then get position values and add into filter
            var positionValues = window.location.pathname.match("pos(.*)");
            if (positionValues[0].includes("/"))
            { positionValues = positionValues[0].split("/"); }
            positionValues = positionValues[0];
            positionValues = positionValues.split("pos-")[1];
            boundryLatlongValues = positionValues;
            //add pos_list values in filter
            filter["pos_lst"] = [positionValues];
            $("#filterCompanyForm").submit();
        }
    }

    function SetCityStateValueInSearchTextbox() {
        $(".searchFilterTextbox").val(cityName + ',' + state);
    }

    //when user close dropdown then send request for apply filter
    $('.entity-dropdown').on('hide.bs.dropdown', function () {
        $(".filterApplyBtnForEntityType").hide();
        var isRequestToSend = false;
        var entityTypesCheckedValues = "";
        $("[class*='entityType-checkbox']input:checked").each(function () {
            if (entityTypesCheckedValues == "") {
                entityTypesCheckedValues = $(this).val(); +",";
            }
            else {
                entityTypesCheckedValues += "," + $(this).val();
            }
        });
        if (entityTypesCheckedValues == "") {
            $("#entitytype-any").prop('checked', true);
            entityTypesCheckedValues = "Any";
            isRequestToSend = false;
        }
        if ($.inArray(entityTypesCheckedValues, filter.entity) != -1) {
        }
        else if (entityTypesCheckedValues == "Any" && filter.entity == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            filter["entity"] = [entityTypesCheckedValues];
            //create url and set values in filter
            CreateUrl('entity', entityTypesCheckedValues);
            $("#filterCompanyForm").submit();

        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    $('.cargo-dropdown').on('hide.bs.dropdown', function ApplyCargoTypeFilters() {
        $(".filterApplyBtnForCargoType").hide();
        var isRequestToSend = false;
        var cargoTypeSelectedItems = "";
        SetCityStateValueInSearchTextbox();
        //get entitytype checkbox values
        $("[name='SelectedCargoTypes']input:checked").each(function () {
            var item = $(this);
            if (cargoTypeSelectedItems == "") {
                cargoTypeSelectedItems = $(this).val(); +","
            }
            else { cargoTypeSelectedItems += "," + $(this).val(); }
        });
        if (cargoTypeSelectedItems == "") {
            $('input[id="cargotype-all"]').prop('checked', true);
            cargoTypeSelectedItems = "All";
            isRequestToSend = false;
        }
        if ($.inArray(cargoTypeSelectedItems, filter.cargo) != -1) {
        }
        else if (cargoTypeSelectedItems == "All" && filter.cargo == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            filter["cargo"] = [cargoTypeSelectedItems];
            CreateUrl('cargo', cargoTypeSelectedItems);
            $("#filterCompanyForm").submit();
        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    $('.trucktractor-dropdown').on('hide.bs.dropdown', function () {
        $(".filterApplyBtnForTruckAndTractor").hide();
        var isRequestToSend = false;
        var trucktractorselectedItems = "";
        //get entitytype checkbox values
        $("[class='trucktractor-checkbox']input:checked").each(function () {
            if (trucktractorselectedItems == "") {
                trucktractorselectedItems = $(this).val(); +","
            }
            else { trucktractorselectedItems += "," + $(this).val(); }
        });
        if (trucktractorselectedItems == "") {
            $('input[id="trucktracktor-any"]').prop('checked', true);
            trucktractorselectedItems = "Any";
            isRequestToSend = false;
        }
        if ($.inArray(trucktractorselectedItems, filter.truckortractor) != -1) {
        }
        else if (trucktractorselectedItems == "Any" && filter.truckortractor == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            filter["truckortractor"] = [trucktractorselectedItems];
            CreateUrl('truckortractor', trucktractorselectedItems);
            $("#filterCompanyForm").submit();
        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    $('.serviceType-dropdown').on('hide.bs.dropdown', function () {
        $(".filterApplyBtnForServiceType").hide();
        var isRequestToSend = false;
        var serviceTypeSelectedItems = "";
        //get entitytype checkbox values
        $("[name='SelectedServiceTypes']input:checked").each(function () {
            if (serviceTypeSelectedItems == "") {
                serviceTypeSelectedItems = $(this).val(); +","
            }
            else { serviceTypeSelectedItems += "," + $(this).val(); }
        });
        if (serviceTypeSelectedItems == "") {
            $('input[id="servictype-all"]').prop('checked', true);
            serviceTypeSelectedItems = "All";
            isRequestToSend = false;
        }
        if ($.inArray(serviceTypeSelectedItems, filter.service) != -1) {
        }
        else if (serviceTypeSelectedItems == "All" && filter.service == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            filter["service"] = [serviceTypeSelectedItems];
            CreateUrl('service', serviceTypeSelectedItems);
            $("#filterCompanyForm").submit();
        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    //Trailer Type
    $('.trailerType-dropdown').on('hide.bs.dropdown', function () {
        $(".filterApplyBtnForTrailerType").hide();
        var isRequestToSend = false;
        var trailerTypeSelectedItems = "";
        SetCityStateValueInSearchTextbox();
        //get trailertype checkbox values
        $("[name='SelectedTrailerTypes']input:checked").each(function () {
            if (trailerTypeSelectedItems == "") {
                trailerTypeSelectedItems = $(this).val(); +","
            }
            else { trailerTypeSelectedItems += "," + $(this).val(); }
        });
        if (trailerTypeSelectedItems == "") {
            $('input[id="trailertype-all"]').prop('checked', true);
            trailerTypeSelectedItems = "Not Interested";
            isRequestToSend = false;
        }
        if ($.inArray(trailerTypeSelectedItems, filter.trailer) != -1) {
        }
        else if (trailerTypeSelectedItems == "Not Interested" && filter.trailer == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            filter["trailer"] = [trailerTypeSelectedItems];
            CreateUrl('trailer', trailerTypeSelectedItems);
            $("#filterCompanyForm").submit();
        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    //Driver Type
    $('.driverType-dropdown').on('hide.bs.dropdown', function () {
        $(".filterApplyBtnForDriverType").hide();
        var isRequestToSend = false;
        var driverTypeSelectedItems = "";
        SetCityStateValueInSearchTextbox();
        //get drivertype checkbox values
        $("[name='SelectedDriverTypes']input:checked").each(function () {
            if (driverTypeSelectedItems == "") {
                driverTypeSelectedItems = $(this).val(); +","
            }
            else { driverTypeSelectedItems += "," + $(this).val(); }
        });
        if (driverTypeSelectedItems == "") {
            $('input[id="drivertype-all"]').prop('checked', true);
            driverTypeSelectedItems = "Not Interested";
            isRequestToSend = false;
        }
        if ($.inArray(driverTypeSelectedItems, filter.driver) != -1) {
        }
        else if (driverTypeSelectedItems == "Not Interested" && filter.driver == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            filter["driver"] = [driverTypeSelectedItems];
            CreateUrl('driver', driverTypeSelectedItems);
            $("#filterCompanyForm").submit();
        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    $('.sort-by-option').on('hide.bs.dropdown', function () {
        $(".filterApplyBtnForSortBy").hide();
        var isRequestToSend = false;
        var selectedItem = $("input[name='SortBy']:checked").val();
        var currentcheckboxvalue = $(this).val();
        SetCityStateValueInSearchTextbox();
        if ($.inArray(selectedItem, filter.sortby) != -1) {
        }
        else if (selectedItem == "Relevance" && filter.sortby == undefined) {
            isRequestToSend = false;
        }
        else {
            isRequestToSend = true;
        }
        if (isRequestToSend) {
            CreateUrl("sortby", selectedItem);
            $(this).prop('checked', true);
            $("#filterCompanyForm").submit();
        }
        var myJSON = JSON.stringify(filter);
        window.localStorage.setItem("filter", myJSON);
    });

    $(".entity-dropdown").click(function () {
        $(".filterApplyBtnForEntityType").show();
    });
    $(".filterApplyBtnForEntityType").click(function () {
        $('.entity-dropdown').trigger('hide.bs.dropdown');
    });

    $(".cargo-dropdown").click(function () {
        $(".filterApplyBtnForCargoType").show();
    });
    $(".filterApplyBtnForCargoType").click(function () {
        $('.cargo-dropdown').trigger('hide.bs.dropdown');
    });
    $(".trucktractor-dropdown").click(function () {
        $(".filterApplyBtnForTruckAndTractor").show();
    });
    $(".filterApplyBtnForTruckAndTractor").click(function () {
        $('.trucktractor-dropdown').trigger('hide.bs.dropdown');
    });
    $(".serviceType-dropdown").click(function () {
        $(".filterApplyBtnForServiceType").show();
    });
    $(".filterApplyBtnForServiceType").click(function () {
        $('.serviceType-dropdown').trigger('hide.bs.dropdown');
    });
    //Trailer type
    $(".trailerType-dropdown").click(function () {
        $(".filterApplyBtnForTrailerType").show();
    });
    $(".filterApplyBtnForTrailerType").click(function () {
        $('.trailerType-dropdown').trigger('hide.bs.dropdown');
    });
    //Driver Type
    $(".driverType-dropdown").click(function () {
        $(".filterApplyBtnForDriverType").show();
    });
    $(".filterApplyBtnForDriverType").click(function () {
        $('.trailerType-dropdown').trigger('hide.bs.dropdown');
    });

    $(".sort-by-option").click(function () {
        $(".filterApplyBtnForSortBy").show();
    });
    $(".filterApplyBtnForSortBy").click(function () {
        $('.sort-by-option').trigger('hide.bs.dropdown');
    });

    $('#globalHiring').change(function () {
        //set filter values in url
        var filterValues = "";
        $.each(filter, function (key, values) {
            filterValues += key + "-" + values + "/";
        });
         
        //create url
        if (cityName.includes(' ')) {
            cityName = cityName.split(" ").join("-");
        }
        url = "/" + state.trim() + "/" + cityName + "/" + filterValues;

        var checkoxIsCheked = $(this).is(':checked');
        var suggestURL = "/storeischeckcheckboxvalue";
         
        $.ajax({
            url: suggestURL,
            data: "{ 'isHiringCheckboxCheck': '" + checkoxIsCheked + "'}",
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            success: function (data) {
                if (state == "" && city == "") {
                    window.location.href = "/";
                }
                else if (city == "" || city == undefined) {
                    window.location.href = "/" + state;
                }
                else {
                    window.location.href = "/" + state.trim() + "/" + cityName + "/" + filterValues;
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
        //set filter values in url
        var filterValues = "";
        $.each(filter, function (key, values) {
            filterValues += key + "-" + values + "/";
        });

        //create url
        if (cityName.includes(' ')) {
            cityName = cityName.split(" ").join("-");
        }
        url = "/" + state.trim() + "/" + cityName + "/" + filterValues;

        var checkoxIsCheked = $(this).is(':checked');
        var suggestURL = "/storereviewsfiltercheckboxvalue";

        $.ajax({
            url: suggestURL,
            data: { isReveiewsCheckboxCheck: checkoxIsCheked },
            type: "POST",
            success: function (data) {
                if (state == "" && city == "") {
                    window.location.href = "/";
                }
                else if (city == "" || city == undefined) {
                    window.location.href = "/" + state;
                }
                else {
                    window.location.href = "/" + state.trim() + "/" + cityName + "/" + filterValues;
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
 
})(this);