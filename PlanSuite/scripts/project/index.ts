import { ajax } from "jquery";
import { Localisation } from "../localisation.js";
import { isBlank } from '../site.js'
import { TimeSpan } from "../timespan.js";
import { ProjectCommon } from "./projectCommon.js"

const localisation = new Localisation();
var projectId: number;
var columnCount: number;
var userId: number;

enum AddMemberResponse {
    Success = 0,
    ServerError,
    IncorrectRoles,
    NoUser,
    AlreadyHasAccess,
    IncorrectTier,
    IncorrectTierYou
}

$(function () {
    projectId = $("#projectId").val() as number;
    columnCount = $("#columnCount").val() as number;
    userId = $("#userId").val() as number;

    $(".draggable").draggable({
        revert: "invalid"
    });

    $('.column').droppable({
        accept: '.draggable',
        drop: function (event, ui) {
            var dropped = ui.draggable;
            var droppedOn = $(this);
            $(dropped).detach().css({ top: 0, left: 0 }).appendTo(droppedOn);

            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                url: "/api/Project/movecard",
                beforeSend: function (request) {
                    request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
                },
                data: JSON.stringify({ cardId: dropped[0].id, columnId: droppedOn[0].id }),
            });
        }
    });

    $("#addColumnBtn").on("click", function () { addColumnBtn(projectId); });
    $("#seeMembersBtn").on("click", viewProjectMembers);
    $("#addMemberBtn").on("click", onAddMember);
    $("#onLeaveProjectBtn").on("click", onLeaveProject);
    $("#viewCardLabel").on("click", ProjectCommon.editName);
    $("#viewCardText").on("click", ProjectCommon.editDescription);
    $("#addChecklistBtn").on("click", ProjectCommon.onAddChecklist);
    $("#editCardSaveContentBtn").on("click", ProjectCommon.onEditCardSaveContent);
    $("#editCardBtn").on("click", ProjectCommon.onEditCard);
    $("#markCompleteBtn").on("click", ProjectCommon.onMarkComplete);

    $.ajax({
        type: "GET",
        dataType: "json",
        url: `/api/Project/GetMilestones?id=${projectId}`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        success: function (response) {
            response.milestones.forEach(function (value) {
                $(`#listMilestonesEditBtn_${value.id}`).on("click", function () { onEditMilestoneBtn(value.id); });
                $(`#listMilestonesCloseBtn_${value.id}`).on("click", function () { onCloseMilestoneBtn(value.id); });
                $(`#listMilestonesDeleteBtn_${value.id}`).on("click", function () { onDeleteMilestoneBtn(value.id); });
            });
        }
    });

    $(".ps-card-item-selector").on("click", function () {
        let arr = $(this).attr("id").match(/[0-9]+$/);
        let id = parseInt(arr[0], 10);
        console.log(id);
        $("#AddTask_ColumnId").val(id);
    });

    //$("#listMilestonesEditBtn").on("click", function () { onEditMilestoneBtn(); });
    //$("#listMilestonesCloseBtn").on("click", onCloseMilestoneBtn);

    for (var i = 0; i < columnCount; i++) {
        let colIdx: JQuery<HTMLElement> = $(`#colIndex_${i}`) as JQuery<HTMLElement>;

        // is there not a better way to get the column id?
        let colId: number = Number(colIdx.children("input[type='hidden']:first").val().toString().split("_")[1]);
        console.log(`colId = ${colId} index ${i}`);

        let column = $(`#Column_${colId}`);
        column.on("click", function () {
            console.log(`Clicked on ${colId}`);
            onClickEditColumnTitle(colId);
        });

        let addNewCardBtn = $(`#addNewCard_${colId}`);
        addNewCardBtn.on("click", function () {
            console.log(`onclick ${colId}`);
            addNewCard(colId);
        });

        let actualColumn: JQuery<HTMLElement> = $(`#${colId}.column.ui-droppable`) as JQuery<HTMLElement>;

        actualColumn.children().each(function () {
            var element = $(this);
            if (!element.hasClass("script-ignore") && !element.is("input")) {
                var id: number = element.attr("id") as unknown as number;
                $(`#viewCardName_${id}`).on("click", function () { ProjectCommon.viewCardButton(id) });
            }
        });
    }
});

function removeCard() {
    const name = "#createCardDiv";
    $(name).off();
    $(name).addClass("d-none");
}

function addNewCard(id) {
    const name = "#createCardDiv";

    //removeCard();

    console.log(`appendTo(#Column_${id})`);
    $(name).detach().appendTo(`#Column_${id}`);
    $(name).removeClass("d-none");

    $('#AddCard_ColumnId').val(id);
    $('#AddCard_ProjectId').val(projectId);
    $('#cardNameField').focus();

    $('#createCardDiv').on("focusout", removeCard);
}

function addColumnBtn(dbId) {
    console.log(dbId);
    $("#AddColumn_ProjectId").val(dbId);
}

function viewProjectMembers() {
    $("#memberList").empty();
    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        url: `/api/Project/getprojectmembers?projectId=` + projectId,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        success: function (response) {
            console.log(response);

            $("#memberList").append(`<li class=\"list-group-item d-flex justify-content-between align-items-start\">\
                        <div class=\"ms-2 me-auto\">\
                            <div class=\"fw-auto\">${response.cardOwner} <span class=\"badge ps-div-projects rounded-pill\">Owner</span></div>\
                            Project Owner\
                        </div>\
                    </li>`);

            console.log(response.cardAdmins);
            if (response.cardAdmins.length > 0) {
                response.cardAdmins.forEach(admin => {
                    console.log(admin);
                    $("#memberList").append(`<li class=\"list-group-item d-flex justify-content-between align-items-start\">\
                            <div class=\"ms-2 me-auto\">\
                                <div class=\"fw-auto\">${admin} <span class=\"badge ps-div-users rounded-pill\">Admin</span></div>\
                                Project Admin\
                            </div>\
                        </li>`);
                });
            }

            console.log(response.cardMembers);
            if (response.cardMembers.length > 0) {
                response.cardMembers.forEach(member => {
                    console.log(member);
                    $("#memberList").append(`<li class=\"list-group-item d-flex justify-content-between align-items-start\">\
                            <div class=\"ms-2 me-auto\">\
                                <div class=\"fw-auto\">${member}</div>\
                                Project Member\
                            </div>\
                        </li>`);
                });
            }
        },
    });
}

function onAddMember()
{
    var input = $("#addMemberInput").val();
    console.log(input);
    var userId: string = $("#userId").val() as string;

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: `/api/Project/addmember`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ projectId: projectId, userId: userId, name: `${input}` }),
        success: function(response) {
            console.log(response);
            if(response.response != AddMemberResponse.Success.valueOf())
            {
                var div = $("#addUserFail");
                div.removeClass("d-none");
                if (response.response == AddMemberResponse.AlreadyHasAccess.valueOf())
                {
                    div.html(localisation.Get("ADD_USER_ALREADY_HAS_ACCESS"));
                }
                else if (response.response == AddMemberResponse.IncorrectRoles.valueOf())
                {
                    div.html(localisation.Get("ADD_USER_INCORRECT_ROLES"));
                }
                else if (response.response == AddMemberResponse.NoUser.valueOf())
                {
                    div.html(localisation.Get("ADD_USER_NO_USER"));
                }
                else if (response.response == AddMemberResponse.IncorrectTier.valueOf())
                {
                    div.html(localisation.Get("ADD_USER_INCORRECT_TIER"));
                }
                else if (response.response == AddMemberResponse.IncorrectTierYou.valueOf())
                {
                    div.html(localisation.Get("ADD_USER_INCORRECT_TIER_YOU"));
                }
                else
                {
                    div.html(localisation.Get("FAIL_SERVER"));
                }
            }
            else
            {
                $("#addUserSuccess").removeClass("d-none");
            }
        },
    });
}

function onClickEditColumnTitle(dbId) {
    var columnTitle = $(`#Column_${dbId}`);
    var columnTitleForm = $(`#ColumnEdit_${dbId}`);
    var columnTitleInput = $(`#ColumnText_${dbId}`);
    var oldTitle = columnTitleInput.val();

    columnTitle.addClass("d-none");
    columnTitleForm.removeClass("d-none");
    columnTitleInput.focusout(function () {
        sendEditColumnNameEvent(dbId, oldTitle);
    });
    columnTitleInput.focus();
}

function sendEditColumnNameEvent(dbId, oldTitle) {
    var columnTitle = $(`#Column_${dbId}`);
    var columnTitleForm = $(`#ColumnEdit_${dbId}`);
    var columnTitleInput = $(`#ColumnText_${dbId}`);

    columnTitleInput.off("focusout");
    columnTitle.removeClass("d-none");
    columnTitleForm.addClass("d-none");
    var text: string = columnTitleInput.val() as string;
    if (!isBlank(text)) {
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/api/Project/EditColumnName",
            beforeSend: function (request) {
                request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
            },
            data: JSON.stringify({ columnId: dbId, columnText: text }),
            success: function (response) {
                columnTitle.html(text);
            },
        });
    }
    else {
        columnTitle.val(oldTitle);
    }
}

function onLeaveProject() {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: `/api/Project/leaveproject`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ projectId: projectId, userId: userId }),
        success: function(response) {
            window.location.replace("/Home");
        },
    });
}

function onEditMilestoneBtn(id) {
    var milestoneId = $(`#listMilestonesEditBtn_${id}`).val();
    $("#EditMilestone_MilestoneId").val(milestoneId);

    $.ajax({
        type: "GET",
        dataType: "json",
        url: `/api/Project/GetMilestoneInfoForEditing?id=${milestoneId}`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        success: function (response) {
            $("#EditMilestone_Title").val(response.title);
            $("#EditMilestone_Description").val(response.description);
            if (response.dueDate != null) {
                $("#EditMilestone_DueDate").val(response.dueDate);
            }
        },
    });
}

function onCloseMilestoneBtn(id) {
    var milestoneCloseBtn = $(`#listMilestonesCloseBtn_${id}`)
    var milestoneId = milestoneCloseBtn.val();
    $("#EditMilestone_MilestoneId").val(milestoneId);
    var listItem = $(`#milestoneListItem_${milestoneId}`);
    var badge = $(`#milestoneBadge_${milestoneId}`);
    var dueDate = $(`#milestoneDueDate_${milestoneId}`);
    var editBtn = $(`#listMilestonesEditBtn_${milestoneId}`);

    $.ajax({
        type: "POST",
        contentType: "application/json",
        dataType: "json",
        url: `/api/Project/ToggleMilestoneIsClosed`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ milestoneId: milestoneId }),
        success: function (response) {
            if (response.isClosed == true) {
                milestoneCloseBtn.text("Reopen");
                listItem.addClass("list-group-item-light");
                badge.removeClass("d-none");
                dueDate.addClass("d-none");
                editBtn.addClass("d-none");
            }
            else {
                milestoneCloseBtn.text("Close");
                listItem.removeClass("list-group-item-light");
                badge.addClass("d-none");
                dueDate.removeClass("d-none");
                editBtn.removeClass("d-none");
            }
        },
    });
}

function onDeleteMilestoneBtn(id) {
    var milestoneDeleteBtn = $(`#listMilestonesDeleteBtn_${id}`)
    var milestoneId = milestoneDeleteBtn.val();
    $("#DeleteMilestone_MilestoneId").val(milestoneId);
}

//