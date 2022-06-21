﻿import { Localisation } from "../localisation.js";
import { isBlank } from '../site.js'

const verificationToken: string = $("#RequestVerificationToken").val() as string;
const localisation = new Localisation();
var projectId: number;
var columnCount: number;
var userId: number;

enum Priority {
    None,
    Low,
    Medium,
    High
};

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
                    request.setRequestHeader("RequestVerificationToken", verificationToken);
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

    for (var i = 0; i < columnCount; i++) {
        var column: JQuery<HTMLElement> = $(`#colIndex_${i}`) as JQuery<HTMLElement>;

        // is there not a better way to get the column id?
        var colId: number = Number(column.children("input[type='hidden']:first").val().toString().split("_")[1]);
        console.log(`colId = ${colId} start`);

        var col = column.children(`#addNewCard_${colId}`);
        console.log(`${col.html()}`);

        $(`#Column_${colId}`).on("click", function () {
            onClickEditColumnTitle(colId)
        });
        $(`#addNewCard_${colId}`).on("click", function () {
            addNewCard(colId);
        });

        var actualColumn: JQuery<HTMLElement> = $(`.column#${colId}`) as JQuery<HTMLElement>;
        //console.log(actualColumn.children());

        actualColumn.children().each(function () {
            var element = $(this);
            if (!element.hasClass("script-ignore") && !element.is("input")) {
                var id: number = element.attr("id") as unknown as number;
                $(`#viewCardName_${id}`).on("click", function () { viewCardButton(id) });
            }
        });
        console.log(`colId = ${colId} end`);
    }

    // 

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
            request.setRequestHeader("RequestVerificationToken", verificationToken);
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
        viewCardButton(dbId);
    });

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/editcardname",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        data: JSON.stringify({ cardId: dbId, name: text }),
    });
}

function removeDateEditor() {
    var input: string = $("#viewCardDueDateDateTime").val() as string;
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

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/EditCardDueDate",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
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

function viewCardButton(dbId) {
    $('#viewCardId').val(dbId);

    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        url: `/api/Project/getcard?cardId=${dbId}`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        success: function (response) {
            var dateString = "None";
            if (response.unixTimestamp > 0) {
                // I have to multiply by 1000 for some reason here idk why
                dateString = new Date(response.unixTimestamp * 1000).toDateString();
            }

            //var localisation = new Localisation();
            
            var priority: string = localisation.Get("NONE");
            if (response.priority == Priority.Low.valueOf())
            {
                priority = `<span class="badge bg-success">${localisation.Get("LOW")}</span>`;
            }
            else if (response.priority == Priority.Medium.valueOf())
            {
                priority = `<span class="badge bg-warning">${localisation.Get("MEDIUM")}</span>`;
            }
            else if (response.priority == Priority.High.valueOf())
            {
                priority = `<span class="badge bg-danger">${localisation.Get("HIGH")}</span>`;
            }

            var assignee: string = localisation.Get("NOBODY");
            console.log(response);
            if (response.assigneeName != "NOBODY") {
                assignee = response.assigneeName;
            }

            var checklistHolder = $("#checklistHolder");
            if (response.cardChecklists.length > 0) {
                checklistHolder.removeClass("d-none");
                checklistHolder.empty();
                response.cardChecklists.forEach(function (element) {
                    addChecklist(element.id, element.checklistName, response.checklistItems);
                })
            }
            else {
                checklistHolder.addClass("d-none");
            }

            $('#viewCardLabel').text(response.name);
            $('#viewCardText').html(response.markdownContent);
            $('#viewCardEditTextEditor').val(response.rawContent);
            $('#viewCardDueDate').html(`<strong>${localisation.Get("VIEW_CARD_DUE_DATE")}</strong> ${dateString}`);
            $('#viewCardPriority').html(`<strong>${localisation.Get("VIEW_CARD_PRIORITY")}</strong> ${priority}`);
            $('#viewCardAssignee').html(`<strong>${localisation.Get("VIEW_CARD_ASSIGNEE")}</strong> ${assignee}`);
        },
    });
}

function addNewCard(id) {
    const name = "#createCardDiv";

    removeCard();

    $(name).detach().appendTo(`#${id}`);
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

// Add mini-form for adding checklist item
function onAddChecklistItem(dbId, divId) {
    var checklistDiv = $(`#${divId}`);
    var inputName = `ChecklistItemInput`;
    var buttonName = `ChecklistItemButton`;

    var input = $(`#${inputName}`);

    if (input.length > 0) {
        input.focus();
        return;
    }

    $(`<div class="row g-2 mb-2" id="insertGroup">\
    <div class="col-auto"><input class="form-control form-control-sm me-0" type="text" name="${inputName}" id="${inputName}"></div>\
    <div class="col-auto"><button type="button" class="btn ps-btn-primary btn-sm ms-0" id="${buttonName}">Insert</button></div>\
    </div>`).insertBefore(`#addItemBtn_${dbId}`);

    input = $(`#${inputName}`);
    console.log(input.attr("id"));
    var button = $(`#${buttonName}`);

    button.click(function () {
        onInsert(dbId, input.val());
    });

    input.keyup(function (event) {
        var id = event.key || event.which || event.keyCode || 0;
        if (id == "Enter" || id == 13) {
            onInsert(dbId, input.val());
        }
    });

    input.focus();
}

function onInsert(dbId, input) {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/AddChecklistItem",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        success: function (response) {
            console.log(response.checklistId == dbId);
            insertChecklistItem(dbId, false, response.id, response.itemName, true);
        },
        data: JSON.stringify({ checklistId: dbId, itemText: input }),
    });
}

function insertChecklistItem(elementId, ticked, id, name, insertBefore = false) {
    var checklistDiv = $(`#Checklist_${elementId}`);
    if (!checklistDiv.length) {
        return;
    }

    var btnGroup = `<div class="btn-group">
    <button class="btn ps-btn-white btn-sm dropdown-toggle ms-1" type="button" data-bs-toggle="dropdown" aria-expanded="false"><i class="bi bi-three-dots"></i></button>\
    <ul class="dropdown-menu">\
    <li><a class="dropdown-item" href="#" id="convertChecklistItemToCardBtn_${id}">Convert to card</a></li>\
    <li><a class="dropdown-item" href="#" id="deleteChecklistItem_${id}">Delete</a></li>\
    </ul>\
    </div>`;

    $(`#convertChecklistItemToCardBtn_${id}`).on("click", function () { convertChecklistItemToCard(id); })
    $(`#deleteChecklistItem_${id}`).on("click", function () { deleteChecklistItem(id); })

    var string = `<div class="form-check mb-1" id="checklistItemFormCheck_${id}">`;
    if (ticked == false) {
        string += `<input class="form-check-input" type="checkbox" name="ChecklistItem_${id}" id="ChecklistItem_${id}"><label class="form-check-label" for="ChecklistItem_${id}" id="ChecklistItemLabel_${id}">${name} ${btnGroup}</label>`;
    }
    else {
        string += `<input class="form-check-input" type="checkbox" name="ChecklistItem_${id}" id="ChecklistItem_${id}" checked><label class="form-check-label" for="ChecklistItem_${id}" id="ChecklistItemLabel_${id}"><s>${name}</s> ${btnGroup}</label>`;
    }
    string += "</div>";
    if (!insertBefore) {
        checklistDiv.append(string);
    }
    else {
        $(string).insertBefore(`#addItemBtn_${elementId}`);
    }

    var insertGroup = $("#insertGroup");
    if (insertGroup.length) {
        insertGroup.remove();
    }

    var item = $(`#ChecklistItemLabel_${id}`);
    $(`input[type=checkbox][name=ChecklistItem_${id}]`).change(function () {
        if ($(this).is(':checked')) {
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                url: "/api/Project/EditChecklistItemTickedState",
                beforeSend: function (request) {
                    request.setRequestHeader("RequestVerificationToken", verificationToken);
                },
                success: function (response) {
                    item.html(`<s>${name}</s>`);
                },
                data: JSON.stringify({ checklistItemId: id, tickedState: true }),
            });
        }
        else {
            $.ajax({
                type: "POST",
                dataType: "json",
                contentType: "application/json",
                url: "/api/Project/EditChecklistItemTickedState",
                beforeSend: function (request) {
                    request.setRequestHeader("RequestVerificationToken", verificationToken);
                },
                success: function (response) {
                    item.html(`${name}`);
                },
                data: JSON.stringify({ checklistItemId: id, tickedState: false }),
            });
        }
    });
}

function convertChecklistItemToCard(dbId) {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/ConvertChecklistItemToCard",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        success: function (response) {
            $(`#checklistItemFormCheck_${dbId}`).remove();
            location.reload();
        },
        data: JSON.stringify({ checklistItemId: dbId }),
    });
}

function deleteChecklistItem(dbId) {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/DeleteChecklistItem",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        success: function (response) {
            $(`#checklistItemFormCheck_${dbId}`).remove();
        },
        data: JSON.stringify({ checklistItemId: dbId }),
    });
}

function deleteChecklist(dbId) {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/DeleteChecklist",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        success: function (response) {
            $(`#Checklist_${dbId}`).remove();
        },
        data: JSON.stringify({ checklistId: dbId }),
    });
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
                request.setRequestHeader("RequestVerificationToken", verificationToken);
            },
            data: JSON.stringify({ id: dbId, name: text }),
            success: function (response) {
                addChecklist(response.id, response.checklistName, null);
            },
        });

        btn.off("click");
    });
}

function addChecklist(id, name, checklistItems) {
    var checklistHolder = $("#checklistHolder");
    checklistHolder.removeClass("d-none");

    checklistHolder.append(`<div class="ps-card-checklist mb-1" id="Checklist_${id}">
        <h6 id="ChecklistName_${id}">${name} <button id="ChecklistNameDeleteBtn_${id}" type="button" class="btn ps-btn-white btn-sm")">Delete</button></h6>`);

    $(`ChecklistNameDeleteBtn_${id}`).on("click", function () { deleteChecklist(id); });

    checklistHolder.append(`<div id="ChecklistHolder_${id}">`);

    var checklistDiv = $(`#Checklist_${id}`);
    if (checklistItems != null && checklistItems.length) {
        checklistItems.forEach(function (checklistItem) {
            if (checklistItem.checklistId == id) {
                insertChecklistItem(id, checklistItem.itemTicked, checklistItem.id, checklistItem.itemName);
            }
        });
    }

    checklistDiv.append(`<button class="btn ps-btn-white" type="button" id="addItemBtn_${id}"><i class="bi bi-plus-circle"></i> Add an item</button>`);
    $(`addItemBtn_${id}`).on("click", function () { onAddChecklistItem(id, checklistDiv.attr("id")); });

    checklistHolder.append(`</div>`);
}

function viewProjectMembers() {
    $("#memberList").empty();
    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        url: `/api/Project/getprojectmembers?projectId=` + projectId,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
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
            request.setRequestHeader("RequestVerificationToken", verificationToken);
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
                request.setRequestHeader("RequestVerificationToken", verificationToken);
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
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        data: JSON.stringify({ projectId: projectId, userId: userId }),
        success: function(response) {
            window.location.replace("/Home");
        },
    });
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
    console.log(`assigneeId: ${assigneeId}`);

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/EditCard",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        data: JSON.stringify({ cardId: dbId, timestamp: dateEntered, priority: radioValue, assigneeId: assigneeId }),
        success: function (response) {
            viewCardButton(dbId);
        },
    });
}

function getCardDueDate() {
    var input: string = $("#viewCardDueDateDateTime").val() as string;
    var dbId: number = $('#viewCardId').val() as number;

    console.log(`getCardDueDate: ${input}`);
    var timestamp = 0;
    var dateEntered = 0;
    if (!isBlank(input)) {
        dateEntered = new Date(input).getTime() / 1000;
    }
    return dateEntered;
}