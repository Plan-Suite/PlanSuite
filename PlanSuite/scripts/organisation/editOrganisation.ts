import { onDeleteInput } from "../site";

$(function () {
    $("#confirmDeleteOrgName").on("keyup", function () { onDeleteInput('deleteButton', 'DeleteOrganisation_Name', 'confirmDeleteOrgName') });
});