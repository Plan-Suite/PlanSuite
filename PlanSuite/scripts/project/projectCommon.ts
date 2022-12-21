import { Localisation } from "../localisation.js";
import { isBlank } from '../site.js'
const localisation = new Localisation();
const verificationToken: string = $("#RequestVerificationToken").val() as string;

export enum EPriority {
    None = 0,
    Low,
    Medium,
    High
};

export class ProjectCommon {
    static onMarkComplete() {
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
        $(`.ps-task-item#DueTask_${cardId}`).fadeOut(1000);
        let boardTask = $(`.ps-board-task#${cardId}`);

        if (boardTask.hasClass("ps-board-task-pending")) {
            boardTask.removeClass("ps-board-task-pending");
            boardTask.addClass("ps-board-task-complete");
        }
        else {
            boardTask.addClass("ps-board-task-pending");
            boardTask.removeClass("ps-board-task-complete");
        }
    }

    static onEditCardCancelEdit() {
        console.log("cancel edit");
        var dbId = $('#viewCardId').val();
        $("#editCardContentForm").addClass("d-none");
        $("#viewCardContent").removeClass("d-none");
        ProjectCommon.viewCardButton(dbId);
    }

    static onEditCardSaveContent() {
        var dbId = $('#viewCardId').val();
        $("#editCardContentForm").addClass("d-none");
        $("#viewCardContent").removeClass("d-none");
        var dateEntered = ProjectCommon.getCardDueDate();
        var radioValue = $("input[name='priority']:checked").val();
        var assigneeId = $("#assignee").val();
        var milestoneId = $("#milestone").val();
        var budget = $("#viewCardBudgetInput").val();

        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/api/Project/EditCard",
            beforeSend: function (request) {
                request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
            },
            data: JSON.stringify({ cardId: dbId, timestamp: dateEntered, priority: radioValue, assigneeId: assigneeId, milestoneId: milestoneId, budget: budget }),
            success: function (response) {
                ProjectCommon.viewCardButton(dbId);
            },
        });
    }

    static getCardDueDate() {
        var input: string = $("#viewCardDueDateDateTime").val() as string;
        var dbId: number = $('#viewCardId').val() as number;

        var timestamp = 0;
        var dateEntered = 0;
        if (!isBlank(input)) {
            dateEntered = new Date(input).getTime() / 1000;
        }
        return dateEntered;
    }

    static onAddChecklist() {
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

    static editName(): void {
        var dbid = $('#viewCardId').val();
        var currentName = $('#viewCardLabel').text();
        console.log(`editName: ${currentName}`);
        $('#viewCardLabel').addClass("d-none");
        $('#viewCardEditLabel').removeClass("d-none");

        $('#viewCardEditLabelEditor').val(currentName);
        $('#viewCardEditLabelEditor').focus();
        $('#viewCardEditLabelEditor').on("focusout", ProjectCommon.removeEditLabel);
    }

    static editDescription(): void {
        var dbid = $('#viewCardId').val();
        $('#viewCardText').addClass("d-none");
        $('#viewCardEditText').removeClass("d-none");

        $('#viewCardEditTextEditor').focus();
        $('#viewCardEditTextEditor').on("focusout", ProjectCommon.removeEditForm);
        $('#viewCardEditTextEditor').keydown(function (event) {
            var id = event.key || event.which || event.keyCode || 0;
            if (id == 13) {
                ProjectCommon.removeEditForm();
            }
        });
    }

    static removeEditLabel() {
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

    static removeDateEditor() {
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

    static removeEditForm() {
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

    static GetVerificationToken(): string {
        return verificationToken;
    }

    static GetProjectMembers(projectId): Map<string, string> {
        let dictionary: Map<string, string> = new Map<string, string>();
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/api/Project/GetProjectMembers",
            beforeSend: function (request) {
                request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
            },
            data: JSON.stringify({ projectId: projectId }),
            success: function (response) {
                Object.entries(response.members).forEach(([k, v]) => {
                    dictionary.set(k, v as string);

                });
            }
        });
        return dictionary;
    }

    static viewCardButton(dbId) {
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
                var endDate = "None";
                var startDate = "None";
                if (response.unixTimestamp > 0) {
                    // I have to multiply by 1000 for some reason here idk why
                    endDate = new Date(response.unixTimestamp * 1000).toDateString();
                }
                if (response.startDate > 0) {
                    startDate = new Date(response.startDate * 1000).toDateString();
                }

                var priority: string = localisation.Get("NONE");
                if (response.priority == EPriority.Low.valueOf()) {
                    priority = `<span class="badge bg-success">${localisation.Get("LOW")}</span>`;
                }
                else if (response.priority == EPriority.Medium.valueOf()) {
                    priority = `<span class="badge bg-warning">${localisation.Get("MEDIUM")}</span>`;
                }
                else if (response.priority == EPriority.High.valueOf()) {
                    priority = `<span class="badge bg-danger">${localisation.Get("HIGH")}</span>`;
                }

                var assignee: string = localisation.Get("NOBODY");
                if (response.assigneeName != "NOBODY") {
                    assignee = response.assigneeName;
                }

                var milestone: string = "None";
                if (response.milestoneId > 0) {
                    milestone = response.milestoneName;
                }

                var budget: string = "None";
                if (response.budget > 0) {
                    var budgetType = "";
                    switch (response.budgetType)
                    {
                        case 2:
                            budgetType = " hours";
                            break;
                        case 3:
                            budgetType = " days";
                            break;
                    }
                    budget = `${response.budgetUnit}${response.budget}${budgetType}`;
                }

                var checklistHolder = $("#checklistHolder");
                if (response.cardChecklists.length > 0) {
                    checklistHolder.removeClass("d-none");
                    checklistHolder.empty();
                    response.cardChecklists.forEach(function (element) {
                        ProjectCommon.addChecklist(element.id, element.checklistName, response.checklistItems);
                    })
                }
                else {
                    checklistHolder.addClass("d-none");
                }


                var logHolder = $("#logHolder");
                logHolder.empty();
                logHolder.append("<hr/>");

                response.auditLogs.forEach(function (log) {
                    logHolder.append(`<div class="ps-card-log"><strong>${log.username}</strong><br />${log.message}<br /><small class="text-muted">${log.created}</small></div>`);
                });

                var project: string;
                var projectId: number;
                project = response.projectName;
                projectId = response.projectId;

                $('#viewCardLabel').text(response.name);
                $('#viewCardText').html(response.markdownContent);
                $('#viewCardEditTextEditor').val(response.rawContent);
                $('#viewCardStartDate').html(`<strong>${localisation.Get("VIEW_CARD_START_DATE")}</strong> ${startDate}`);
                $('#viewCardDueDate').html(`<strong>${localisation.Get("VIEW_CARD_DUE_DATE")}</strong> ${endDate}`);
                $('#viewCardPriority').html(`<strong>${localisation.Get("VIEW_CARD_PRIORITY")}</strong> ${priority}`);
                $('#viewCardAssignee').html(`<strong>${localisation.Get("VIEW_CARD_ASSIGNEE")}</strong> ${assignee}`);
                $('#viewCardProject').html(`<strong>${localisation.Get("VIEW_CARD_PROJECT")}</strong> <a class="ps-link-primary" href="/projects/${projectId}">${project}</a>`);
                $('#viewCardMilestone').html(`<strong>${localisation.Get("VIEW_CARD_MILESTONE")}</strong> ${milestone}`);
                $('#viewCardBudget').html(`<strong>${localisation.Get("VIEW_CARD_BUDGET")}</strong> ${budget}`);
            },
        });
    }

    static addChecklist(id, name, checklistItems) {
        var checklistHolder = $("#checklistHolder");
        checklistHolder.removeClass("d-none");

        checklistHolder.append(`<div class="ps-card-checklist mb-1" id="Checklist_${id}">
            <h6 id="ChecklistName_${id}">${name} <button id="ChecklistNameDeleteBtn_${id}" type="button" class="btn ps-btn-white btn-sm")">Delete</button></h6>`);

        $(`#ChecklistNameDeleteBtn_${id}`).on("click", function () { ProjectCommon.deleteChecklist(id); });

        checklistHolder.append(`<div id="ChecklistHolder_${id}">`);

        var checklistDiv = $(`#Checklist_${id}`);
        if (checklistItems != null && checklistItems.length) {
            checklistItems.forEach(function (checklistItem) {
                if (checklistItem.checklistId == id) {
                    ProjectCommon.insertChecklistItem(id, checklistItem.itemTicked, checklistItem.id, checklistItem.itemName);
                }
            });
        }

        checklistDiv.append(`<button class="btn ps-btn-white" type="button" id="addItemBtn_${id}"><i class="bi bi-plus-circle"></i> Add an item</button>`);
        $(`#addItemBtn_${id}`).on("click", function () { ProjectCommon.onAddChecklistItem(id, checklistDiv.attr("id")); });

        checklistHolder.append(`</div>`);
    }

    static deleteChecklist(dbId) {
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

    static insertChecklistItem(elementId, ticked, id, name, insertBefore = false) {
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

        $(document).on('click', `#convertChecklistItemToCardBtn_${id}`, function () { ProjectCommon.convertChecklistItemToCard(id); });
        $(document).on('click', `#deleteChecklistItem_${id}`, function () { ProjectCommon.deleteChecklistItem(id); });

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

    // Add mini-form for adding checklist item
    static onAddChecklistItem(dbId, divId) {
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
            ProjectCommon.onInsert(dbId, input.val());
        });

        input.keyup(function (event) {
            var id = event.key || event.which || event.keyCode || 0;
            if (id == "Enter" || id == 13) {
                ProjectCommon.onInsert(dbId, input.val());
            }
        });

        input.focus();
    }

    static onInsert(dbId, input) {
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
                ProjectCommon.insertChecklistItem(dbId, false, response.id, response.itemName, true);
            },
            data: JSON.stringify({ checklistId: dbId, itemText: input }),
        });
    }

    static convertChecklistItemToCard(dbId) {
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

    static deleteChecklistItem(dbId) {
        console.log(dbId);
        $.ajax({
            type: "POST",
            dataType: "json",
            contentType: "application/json",
            url: "/api/Project/DeleteChecklistItem",
            beforeSend: function (request) {
                request.setRequestHeader("RequestVerificationToken", verificationToken);
            },
            success: function (response) {
                console.log(response);
                $(`#checklistItemFormCheck_${dbId}`).remove();
            },
            data: JSON.stringify({ checklistItemId: dbId }),
        });
    }

    static onEditCard() {
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

/*
    static addTaskLabels(cardName: string, overdue: string, priority: EPriority, milestone: string): string {

        let overdueOut: string;
        if (overdue.length > 0) {
            overdueOut = overdue;
        }

        let priorityOut: string;
        if (priority != EPriority.None) {
            switch (priority) {
                case EPriority.Low:
                    priorityOut = `<span class=\"badge bg-success\">${localisation.Get("LOW")}</span>`;
                    break;
                case EPriority.Medium:
                    priorityOut = `<span class=\"badge bg-warning\">${localisation.Get("MEDIUM")}</span>`;
                    break;
                case EPriority.High:
                    priorityOut = `<span class=\"badge bg-danger\">${localisation.Get("HIGH")}</span>`;
                    break;
            }
        }

        let milestoneOut: string;
        if (milestone.length > 0) {
            milestoneOut = `<span class=\"fw-bold\"><i class=\"bi bi-signpost\"></i> ${milestone}</span>`;
        }

        if (priorityOut.length <= 1 || milestoneOut.length <= 1) {
            priorityOut += "<br/>";
        }

        let cardNameOut = cardName.replace("\n", "");

        return `${milestoneOut} ${cardNameOut} ${overdueOut} ${priorityOut}`;
    }*/
}