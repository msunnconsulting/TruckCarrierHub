(function (global) {

    var vm = global.vm = {};

    
 

    vm.OnSuccessAddUpdateEmail = OnSuccessAddUpdateEmail;
    vm.OnFailureAddUpdateEmail = OnFailureAddUpdateEmail;
    vm.OnBeginAddUpdateEmail = OnBeginAddUpdateEmail;


 


   

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

            //Initialize ckeditor for email modal pop-up
            CKEDITOR.replace('Content', {

            });

            CKEDITOR.instances.Content.setData($("#Content").val());
 
            //Set ckEditor value blank/null/empty when modal closes
            //CKEDITOR.instances.Content.setData("asdfhkjasd hfkjas dhfasdf dasfklj hasdfkj h");
            $("#emailForm").submit();
        });
    }

 
 

    function OnSuccessAddUpdateEmail() {
        var emailId = $("#EmailID").val();
        if (emailId == '' || emailId == undefined) {
            window.location.href = "/admin/business/email?create=true"
        }
        else {
            window.location.href = "/admin/business/email?update=true"
        }
    }

    //Call on Add/Edit business on failure
    function OnFailureAddUpdateEmail(XMLHttpRequest, textStatus, errorThrown) { }


    function OnBeginAddUpdateEmail() {

        $("span[data-valmsg-for='Content']").text("")

        //Get inputed data from ckEditor to save get it into controller
        var ckEditorText = CKEDITOR.instances.Content.getData();

        if (ckEditorText != '' && ckEditorText != undefined) {
            $("#Content").val(ckEditorText)
            $("#AddUpdateEmailForm").submit()
        }
        else {
            $("span[data-valmsg-for='Content']").text("Please Enter Email Content")
        }
    }





})(this);