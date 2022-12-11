﻿import { ProjectCommon } from "../project/projectCommon.js";
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

            $(`#passButtonInfoBtn_${id}`).on("click", function () { passButtonInfo(id, name, desc, date, organisation); });
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
    $("#editCardBtn").on("click", ProjectCommon.onEditCard);
    $("#markCompleteBtn").on("click", ProjectCommon.onMarkComplete);

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