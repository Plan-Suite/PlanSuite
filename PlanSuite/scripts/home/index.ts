import { ProjectCommon } from "../project/projectCommon.js";
import { onDeleteInput } from "../site.js";

const verificationToken: string = $("#RequestVerificationToken").val() as string;

$(function () {

    $("#MyProjects").children().each(function () {
        var element = $(this);
        if (element.attr("id").startsWith("OwnedProject_") || element.attr("id").startsWith("MemberProject_")) {
            var id: number = element.children("#projectId").val() as number;
            var name: string = element.children("#projectName").val() as string;
            var desc: string = element.children("#projectDesc").val() as string;
            var date: string = element.children("#projectDate").val() as string | "";
            var organisation: number = element.children("#projectOrganisation").val() as number;
            var client: string = element.children("#projectClient").val() as string;
            var budget: number = element.children("#projectBudget").val() as number;
            var budgetType: number = element.children("#projectBudgetType").val() as number;
            var budgetUnit: number = element.children("#projectBudgetUnit").val() as number;

            $(`#passButtonInfoBtn_${id}`).on("click", function () { passButtonInfo(id, name, desc, date, organisation, client, budget, budgetType, budgetUnit); });
            $(`#passDeleteButtonInfoBtn_${id}`).on("click", function () { passDeleteButtonInfo(id, name); });
        }
    });

    $("#MyTask").children().each(function () {
        var element = $(this);
        if (element.attr("id").startsWith("DueTask_"))
        {
            var id: number = element.children("#TaskId").val() as number;
            $(`#TaskCheckbox_${id}`).on("click", function ()
            {
                if ($(this).is(':checked')) {
                    $.ajax({
                        type: "GET",
                        dataType: "json",
                        url: `/api/Task/ArchiveTask?taskId=${id}`,
                        beforeSend: function (request) {
                            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
                        }
                    });
                    $(`#DueTask_${id}`).fadeOut(1000);
                }
            });

            let arr = $(this).attr("id").match(/[0-9]+$/);
            let textId = parseInt(arr[0], 10);
            let dueTaskText = $(this).children(`#DueTaskText_${textId}`).first();

            dueTaskText.on("click", function () {
                console.log(`view card ${textId}`);
                ProjectCommon.viewCardButton(id);
            });
        }
    });
    $("#viewCardLabel").on("click", ProjectCommon.editName);
    $("#viewCardText").on("click", ProjectCommon.editDescription);
    $("#addChecklistBtn").on("click", ProjectCommon.onAddChecklist);
    $("#editCardSaveContentBtn").on("click", ProjectCommon.onEditCardSaveContent);
    $("#editCardCancelEditBtn").on("click", ProjectCommon.onEditCardCancelEdit);
    $("#editCardBtn").on("click", ProjectCommon.onEditCard);
    $("#markCompleteBtn").on("click", ProjectCommon.onMarkComplete);

    $(function () {
        $("#confirmDeleteProjName").on("keyup", function () { onDeleteInput('deleteButton', 'DeleteProject_Name', 'confirmDeleteProjName') });
    });
});

function passButtonInfo(dbId, name, desc, date, organisation, client, budget, budgetType, budgetUnit) {
    $("#editProjectLabel").html(`<strong>Edit:</strong> ${name}`);
    $('#editProjName').val(name);
    $('#editProjDesc').val(desc);
    $('#editProjDate').val(date); // FIXME: DueDate does not get assigned
    $('#editProjOrg').val(organisation);
    $('#EditProject_Id').val(dbId);
    $('#editProjClient').val(client);
    $('#editProjBudget').val(Number(budget).toFixed(2)); // FIXME: Budget related items do not get assigned
    $('#editProjBudgetType').val(budgetType);
    $('#editProjBudgetUnit').val(budgetUnit);
}

function passDeleteButtonInfo(dbId, name) {
    $('#deleteProjectLabel').html(`<strong>Delete:</strong> ${name}`);
    $('#DeleteProject_Name').val(name);
    $('#DeleteProject_Id').val(dbId);
}