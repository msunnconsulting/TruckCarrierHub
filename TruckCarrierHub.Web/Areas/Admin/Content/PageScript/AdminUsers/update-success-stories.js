(function (global) {

    var vm = global.vm = {};

    vm.SaveSuccessStories = SaveSuccessStories;
    vm.OnSuccessUpdateSuccessStory = OnSuccessUpdateSuccessStory;
    vm.OnFailureUpdateSuccessStory = OnFailureUpdateSuccessStory;

    init();
    function init() {
        $(document).ready(function () {

            //Initialize ckeditor for email modal pop-up
            CKEDITOR.replace('Content', {
            });

            CKEDITOR.instances.Content.setData($("#Content").val());
        });
    }

    //On Success Success Story
    function OnSuccessUpdateSuccessStory() {
        showAlertMessage("#dvNotification", "success", "Success Story updated successfully.");
    }

    //Call on  Update Success Story on failure
    function OnFailureUpdateSuccessStory(XMLHttpRequest, textStatus, errorThrown) { }


    function SaveSuccessStories() {
        //Get inputed data from ckEditor to save get it into controller
        var ckEditorText = CKEDITOR.instances.Content.getData();
        $("#Content").val(ckEditorText);
        $("#SaveSuccessStoriesForm").submit();
    }





})(this);