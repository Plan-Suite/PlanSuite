import { isBlank, intToTier, arrayToString } from "../site.js";

let editIdInput: JQuery<HTMLElement>;

$(function () {

    editIdInput = $("#EditUserId");
    editIdInput.val(0);

    $("#giveAdminBtn").on("click", giveAdmin);
    $("#sendPasswordResetBtn").on("click", sendPasswordReset);
    $("#saveChangesBtn").on("click", saveChanges);
    $("#onSearchBtn").on("click", onSearch);
    $("#setPlusRole").on("click", function () { setRole(1); });
    $("#setProRole").on("click", function () { setRole(2); });
});

function saveChanges() {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Admin/SaveUserChanges",
        data: JSON.stringify({
            id: $("#EditUserId").val(),
            newName: $("#editUsername").val(),
            newEmail: $("#editEmail").val(),
        }),
        success: function (result) {
            location.reload();
        }
    });
}

function onEdit(id) {
    editIdInput.val(id);
}

function giveAdmin() {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Admin/GiveAdmin",
        data: JSON.stringify({
            id: $("#EditUserId").val(),
        }),
        success: function (result) {
            location.reload();
        }
    });
}

function setRole(role: number) {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Admin/SetRole",
        data: JSON.stringify({
            id: $("#EditUserId").val(),
            role: role,
        }),
        success: function (result) {
            location.reload();
        }
    });
}

function sendPasswordReset() {
    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Admin/SendPasswordReset",
        data: JSON.stringify({
            id: $("#EditUserId").val(),
        }),
        success: function (result) {
            location.reload();
        }
    });
}

function onSearch() {
    var username = $("#username").val();
    if (isBlank(username)) {
        username = "null";
    }

    var email = $("#email").val();
    if (isBlank(email)) {
        email = "null";
    }

    if (email == "null" && username == "null") {
        $("#emailUserError").removeClass("d-none");
        return;
    }

    $("#tableSpinner").removeClass("d-none");
    $("#tbody").empty();

    const url = `/api/Admin/GetUser?username=${username}&email=${email}`;
    console.log(url);

    $.ajax({
        type: "GET",
        dataType: "json",
        contentType: "application/json",
        url: url,
        success: function (result) {
            if (result.getUserModels != null) {
                result.getUserModels.forEach(function (element) {
                    console.log(element);
                    var tr = `<tr>\
                      <th scope="row">${element.username}</th>\
                      <td>${element.email}</td>\
                      <td>${intToTier(element.paymentTier)}</td>\
                      <td>${arrayToString(element.roles)}</td>\
                      <td><button class="btn ps-btn-primary" type="button" id="onEditBtn_${element.userId}" data-bs-toggle="modal" data-bs-target="#userEditModel">Edit</button></td>\
                    </tr>`;
                    $("#tbody").prepend(tr);

                    $(`#onEditBtn_${element.userId}`).on("click", function () { onEdit(element.userId); });
                });
            }
            $("#userSearchTable").removeClass("d-none");
            $("#tableSpinner").addClass("d-none");
        }
    });
}