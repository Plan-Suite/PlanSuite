import { onDeleteInput } from "../site.js";

$(function () {

    $("#MyProjects").children().each(function () {
        var element = $(this);
        if (element.attr("id").startsWith("OwnedProject_")) {
            var id: number = element.children("#projectId").val() as number;
            var name: string = element.children("#projectName").val() as string;
            var desc: string = element.children("#projectDesc").val() as string;
            var date: string = element.children("#projectDate").val() as string | "";

            $(`#passButtonInfoBtn_${id}`).on("click", function () { passButtonInfo(id, name, desc, date); });
            $(`#passDeleteButtonInfoBtn_${id}`).on("click", function () { passDeleteButtonInfo(id, name); });
        }
    });

    $(function () {
        $("#confirmDeleteProjName").on("keyup", function () { onDeleteInput('deleteButton', 'DeleteProject_Name', 'confirmDeleteProjName') });
    });
});

function passButtonInfo(dbId, name, desc, date) {
    $("#editProjectLabel").html(`<strong>Edit:</strong> ${name}`);
    $('#editProjName').val(name);
    $('#editProjDesc').val(desc);
    $('#editProjDate').val(date); // i have no fking clue how to get this working :(
    $('#EditProject_Id').val(dbId);
}

function passDeleteButtonInfo(dbId, name) {
    $('#deleteProjectLabel').html(`<strong>Delete:</strong> ${name}`);
    $('#DeleteProject_Name').val(name);
    $('#DeleteProject_Id').val(dbId);
}