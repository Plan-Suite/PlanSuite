import { Localisation } from "../localisation.js";
const localisation = new Localisation();
const verificationToken: string = $("#RequestVerificationToken").val() as string;

export enum EPriority {
    None = 0,
    Low,
    Medium,
    High
};

export class ProjectCommon {
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

                $('#viewCardLabel').text(response.name);
                $('#viewCardText').html(response.markdownContent);
                $('#viewCardEditTextEditor').val(response.rawContent);
                $('#viewCardStartDate').html(`<strong>${localisation.Get("VIEW_CARD_START_DATE")}</strong> ${startDate}`);
                $('#viewCardDueDate').html(`<strong>${localisation.Get("VIEW_CARD_DUE_DATE")}</strong> ${endDate}`);
                $('#viewCardPriority').html(`<strong>${localisation.Get("VIEW_CARD_PRIORITY")}</strong> ${priority}`);
                $('#viewCardAssignee').html(`<strong>${localisation.Get("VIEW_CARD_ASSIGNEE")}</strong> ${assignee}`);
                $('#viewCardMilestone').html(`<strong>${localisation.Get("VIEW_CARD_MILESTONE")}</strong> ${milestone}`);
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