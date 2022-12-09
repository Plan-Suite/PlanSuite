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
    $("#viewCardLabel").on("click", editName);
    $("#viewCardText").on("click", editDescription);
    $("#addChecklistBtn").on("click", onAddChecklist);
    $("#editCardSaveContentBtn").on("click", onEditCardSaveContent);
    $("#editCardBtn").on("click", onEditCard);
    $("#markCompleteBtn").on("click", onMarkComplete);

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

function removeEditForm() {
    var dbId = $('#viewCardId').val();
    const name = "#viewCardEditText";
    $("#viewCardEditTextEditor").off();
    $(name).addClass("d-none");
    $("#viewCardText").removeClass("d-none");

    const text: string = $("#viewCardEditTextEditor").val() as string;

    $("#viewCardText").text(text);

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/editcarddesc",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ cardId: dbId, description: text }),
    });
}

function removeEditLabel() {
    var dbId = $('#viewCardId').val();
    const name = "#viewCardEditLabel";
    $("#viewCardEditLabelEditor").off();
    $(name).addClass("d-none");
    $("#viewCardLabel").removeClass("d-none");

    var text: string = $("#viewCardEditLabelEditor").val() as string;
    if (isBlank(text)) {
        text = "Card Name";
    }

    var desc = $("#viewCardText").text();
    $("#viewCardLabel").text(text);
    $(`#viewCardName_${dbId}`).text(text);
    $(`#viewCardName_${dbId}`).attr("onclick", function () {
        ProjectCommon.viewCardButton(dbId);
    });

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/editcardname",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ cardId: dbId, name: text }),
    });
}

function removeDateEditor() {
    var input: string = $("#viewCardDueDateDateTime").val() as string;
    var startInput: string = $("#viewCardStartDateDateTime").val() as string;
    var dbId: number = $('#viewCardId').val() as number;
    $("#viewCardDueDateDateTime").off();
    $("#viewCardDueDateEditor").addClass("d-none");
    $("#viewCardDueDate").removeClass("d-none");

    console.log(`removeDateEditor: ${input}`);
    var timestamp = 0;
    var dateEntered;
    if (isBlank(input)) {
        dateEntered = 0;
    }
    else {
        dateEntered = new Date(input).getTime() / 1000;
    }

    var dateString: string = "None";
    if (dateEntered > 0) {
        dateString = `${new Date(dateEntered).toDateString()}`;
    }

    $("#viewCardDueDate").text(`<strong>Due By:</strong> ${dateString}`);

    if (isBlank(startInput)) {
        dateEntered = 0;
    }
    else {
        dateEntered = new Date(startInput).getTime() / 1000;
    }

    var startDateString: string = "None";
    if (dateEntered > 0) {
        startDateString = `${new Date(dateEntered).toDateString()}`;
    }

    $("#viewCardStartDate").text(`<strong>Start Date:</strong> ${startDateString}`);

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/EditCardDueDate",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ cardId: dbId, timestamp: dateEntered }),
    });
}

function editName(): void {
    var dbid = $('#viewCardId').val();
    var currentName = $('#viewCardLabel').text();
    console.log(`editName: ${currentName}`);
    $('#viewCardLabel').addClass("d-none");
    $('#viewCardEditLabel').removeClass("d-none");

    $('#viewCardEditLabelEditor').val(currentName);
    $('#viewCardEditLabelEditor').focus();
    $('#viewCardEditLabelEditor').on("focusout", removeEditLabel);
}

function editDescription(): void {
    var dbid = $('#viewCardId').val();
    $('#viewCardText').addClass("d-none");
    $('#viewCardEditText').removeClass("d-none");

    $('#viewCardEditTextEditor').focus();
    $('#viewCardEditTextEditor').on("focusout", removeEditForm);
    $('#viewCardEditTextEditor').keydown(function (event) {
        var id = event.key || event.which || event.keyCode || 0;
        if (id == 13) {
            removeEditForm();
        }
    });
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

function onAddChecklist() {
    var addChecklistForm = $("#addChecklistForm");
    var input = $("#addChecklistInput");
    var btn = $("#addBtn");
    addChecklistForm.removeClass("d-none");
    $("#addChecklistBtn").addClass("d-none");
    var dbId = $("#viewCardId").val();

    input.focus();
    btn.on("click", function () {
        addChecklistForm.addClass("d-none");
        $("#addChecklistBtn").removeClass("d-none");

        var text: string = input.val() as string;

        if (!text.length) {
            return;
        }

        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/api/Project/AddChecklist",
            beforeSend: function (request) {
                request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
            },
            data: JSON.stringify({ id: dbId, name: text }),
            success: function (response) {
                ProjectCommon.addChecklist(response.id, response.checklistName, null);
            },
        });

        btn.off("click");
    });
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

function onMarkComplete() {
    var cardId = $("#viewCardId").val();
    $.ajax({
        type: "GET",
        dataType: "json",
        url: `/api/Task/ArchiveTask?taskId=${cardId}`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        }
    });
    $(`.card#${cardId}`).fadeOut(1000);
}

function onEditCard() {
    var cardId = $("#viewCardId").val();
    $("#editCardContentForm").removeClass("d-none");
    $("#viewCardContent").addClass("d-none");

    const url = `/api/Project/getcard?cardId=${cardId}`;

    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        url: url,
        success: function (response) {

            // Due Date
            var date = "";
            if (response.unixTimestamp > 0) {
                // I have to multiply by 1000 for some reason here idk why
                date = new Date(response.unixTimestamp * 1000).toDateString();
            }

            $("#viewCardDueDateDateTime").val(date);

            var startDate = "";
            if (response.startDate > 0) {
                // I have to multiply by 1000 for some reason here idk why
                date = new Date(response.startDate * 1000).toDateString();
            }

            $("#viewCardStartDateDateTime").val(startDate);

            // get assignee
            $("#assignee").empty();
            $("#assignee").append("<option value=\"0\">Unassigned</option>");

            Object.entries(response.members).forEach(([k, v]) => {
                $("#assignee").append(`<option value="${k}">${v}</option>`);
            });

            // set assignee
            var guid = "0";
            if (!isBlank(response.assigneeId)) {
                guid = response.assigneeId;
            }

            $("#assignee").val(guid).change();

            // get milestones
            //projectMilestones
            $("#milestone").empty();
            $("#milestone").append("<option value=\"0\">None</option>");

            Object.entries(response.projectMilestones).forEach(([k, v]) => {
                $("#milestone").append(`<option value="${k}">${v}</option>`);
            });

            // set milestone
            var milestoneId = 0;
            if (response.milestoneId > 0) {
                milestoneId = response.milestoneId;
            }

            $("#milestone").val(milestoneId).change();
        }
    });
}

function onEditCardSaveContent() {
    var dbId = $('#viewCardId').val();
    $("#editCardContentForm").addClass("d-none");
    $("#viewCardContent").removeClass("d-none");
    var dateEntered = getCardDueDate();
    var radioValue = $("input[name='priority']:checked").val();
    var assigneeId = $("#assignee").val();
    var milestoneId = $("#milestone").val();

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/EditCard",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: JSON.stringify({ cardId: dbId, timestamp: dateEntered, priority: radioValue, assigneeId: assigneeId, milestoneId: milestoneId }),
        success: function (response) {
            ProjectCommon.viewCardButton(dbId);
        },
    });
}

function getCardDueDate() {
    var input: string = $("#viewCardDueDateDateTime").val() as string;
    var dbId: number = $('#viewCardId').val() as number;

    var timestamp = 0;
    var dateEntered = 0;
    if (!isBlank(input)) {
        dateEntered = new Date(input).getTime() / 1000;
    }
    return dateEntered;
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