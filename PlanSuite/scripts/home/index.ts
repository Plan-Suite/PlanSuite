import { onDeleteInput } from "../site.js";

$(function () {

    $("#MyProjects").children().each(function () {
        var element = $(this);
        if (element.attr("id").startsWith("OwnedProject_") || element.attr("id").startsWith("MemberProject_")) {
            var id: number = element.children("#projectId").val() as number;
            var name: string = element.children("#projectName").val() as string;
            var desc: string = element.children("#projectDesc").val() as string;
            var date: string = element.children("#projectDate").val() as string | "";
            var organisation: number = element.children("#projectOrganisation").val() as number;

            $(`#passButtonInfoBtn_${id}`).on("click", function () { passButtonInfo(id, name, desc, date, organisation); });
            $(`#passDeleteButtonInfoBtn_${id}`).on("click", function () { passDeleteButtonInfo(id, name); });
        }
    });
    $(function () {
        $("#confirmDeleteProjName").on("keyup", function () { onDeleteInput('deleteButton', 'DeleteProject_Name', 'confirmDeleteProjName') });
    });
});

function passButtonInfo(dbId, name, desc, date, organisation) {
    $("#editProjectLabel").html(`<strong>Edit:</strong> ${name}`);
    $('#editProjName').val(name);
    $('#editProjDesc').val(desc);
    $('#editProjDate').val(date); // i have no fking clue how to get this working :(
    $('#editProjOrg').val(organisation);
    $('#EditProject_Id').val(dbId);
}

function passDeleteButtonInfo(dbId, name) {
    $('#deleteProjectLabel').html(`<strong>Delete:</strong> ${name}`);
    $('#DeleteProject_Name').val(name);
    $('#DeleteProject_Id').val(dbId);
}

function onDeleteInput() {
    const button = $('#deleteButton');
    const projName = $('#DeleteProject_Name').val();
    const deleteProjConfirm = $('#confirmDeleteProjName');
    if (deleteProjConfirm.val() == projName) {
        button.removeAttr("disabled");
    }
}