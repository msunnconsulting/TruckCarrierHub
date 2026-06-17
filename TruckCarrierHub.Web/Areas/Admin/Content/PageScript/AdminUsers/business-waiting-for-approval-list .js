(function (global) {

    var vm = global.vm = {};
    vm.pageSort = pageSort;
    vm.OnSuccessBindBusinessOrWaitingForApprovalList = OnSuccessBindBusinessOrWaitingForApprovalList;
    vm.OnFailureBusinessOrWaitingForApprovalList = OnFailureBusinessOrWaitingForApprovalList;
    vm.ApproveBusiness = ApproveBusiness;
    vm.OnSuccessWebsiteApprove = OnSuccessWebsiteApprove;
    vm.OnFailureWebsiteApprove = OnFailureWebsiteApprove;

    $(document).ready(function () {
        $("#businessOrWaitingForApprovalListForm").submit();
    });

    function pageSort(url) {
        $("#businessOrWaitingForApprovalListForm").attr('action', url);
        $("#businessOrWaitingForApprovalListForm").submit();
    }

    function OnSuccessBindBusinessOrWaitingForApprovalList()
    {
    }

    //Approve Website
    function ApproveBusiness(ApprovedId) {
            $("#ApprovedId").val(ApprovedId);
            $("#approveBusinessForm").submit();
    }

    //On success after website is approved
    function OnSuccessWebsiteApprove() {
        showAlertMessage("#dvNotification", "success", "Website Approved successfully.")
        $("#businessSearchListForm").submit();
    }

    //On failure after website fail to approve
    function OnFailureWebsiteApprove()
    { }

    function OnFailureBusinessOrWaitingForApprovalList()
    {
        alert("ERROR")
    }
    

})(this);