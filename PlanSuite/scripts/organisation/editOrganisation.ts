import { onDeleteInput } from "../site.js";

$(function () {
    $("#confirmDeleteOrgName").on("keyup", function () { onDeleteInput('deleteButton', 'DeleteOrganisation_Name', 'confirmDeleteOrgName') });
});