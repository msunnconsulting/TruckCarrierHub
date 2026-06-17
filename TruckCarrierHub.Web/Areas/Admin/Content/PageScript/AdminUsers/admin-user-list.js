(function (global) {

    var vm = global.vm = {};
    vm.pageSort = pageSort;
    vm.OnSuccessBindAdminUserList = OnSuccessBindAdminUserList;
    vm.OnFailureBindAdminUserList = OnFailureBindAdminUserList;
    vm.DeleteAdminUser = DeleteAdminUser;
    vm.OnSuccessDeleteAdminUserById = OnSuccessDeleteAdminUserById;
    vm.OnFailureDeleteAdminUserById = OnFailureDeleteAdminUserById;
    vm.searchAdmin = searchAdmin;

    $(document).ready(function () {
        //Get parameter value using query string
        var Update = getUrlParameter('update');
        var Create = getUrlParameter('create');
        //if parameter is create then display create message
        if (Create != undefined && Create != '' && Create == "true") {
            showAlertMessage("#dvNotification", "success", "Admin user added successfully.")
        }

        //if parameter is update then display update message
        if (Update != undefined && Update != '' && Update == "true") {
            showAlertMessage("#dvNotification", "success", "Admin user updated successfully.")
        }
        $("#adminUserListForm").submit();
    });

    function pageSort(url) {
        $("#adminUserListForm").attr('action', url);
        $("#adminUserListForm").submit();
    }

    function OnSuccessBindAdminUserList()
    { }

    function OnFailureBindAdminUserList()
    {
        alert("ERROR")
    }

    //delete menu
    function DeleteAdminUser(AdminID) {
        if (confirm('Are you sure you want to delete this admin user?')) {
            $("#AdminID").val(AdminID);
            $("#deleteAdminUserForm").submit();
        }
    }

    function OnSuccessDeleteAdminUserById() {
        showAlertMessage("#dvNotification", "success", "Admin user deleted successfully.")
        $("#adminUserListForm").submit();
    }

    function OnFailureDeleteAdminUserById()
    { }

    function searchAdmin() {
        $("#adminUserListForm").attr('action', "/admin/user/list-partial");
    }

})(this);