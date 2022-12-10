let memberList: JQuery<HTMLElement>;
let memberBtns;
const verificationToken: string = $("#RequestVerificationToken").val() as string;
let spinner;

$(function () {
    memberList = $("#memberList");
    memberBtns = $('[id^="btnMembersOrg_"]');
    spinner = $("#spinner");
    console.log(memberBtns.length);
    memberBtns.each(function () {
        let element = $(this);
        let dbId = getDbIdFromId(element.attr("id"));
        element.on("click", function () { onSeeOrganisationMembers(dbId); })
        console.log(`Added click for ${dbId}`);
    });
});

function getDbIdFromId(id: string): number {
    let _tempId: string = id.substring(id.indexOf('_') + 1);
    return parseInt(_tempId);
}

function onSeeOrganisationMembers(dbId: number): void {
    console.log(`onSeeOrganisationMembers(${dbId})`);
    spinner.removeClass("d-none");

    $.ajax({
        type: "GET",
        dataType: "json",
        url: `/api/Organisation/GetOrganisationMembers?orgId=${dbId}`,
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", verificationToken);
        },
        success: function (response) {
            spinner.addClass("d-none");
            memberList.empty();

            if (response.owners.length > 0) {
                response.owners.forEach(function (element) {
                    memberList.append(`<li class=\"list-group-item d-flex justify-content-between align-items-start\">\
                            <div class=\"ms-2 me-auto\">\
                                <div class=\"fw-auto\">${element} <span class=\"badge ps-div-projects rounded-pill\">Owner</span></div>\
                                Owner\
                            </div>\
                        </li>`);
                    console.log(`Owner ${element}`)
                });
            }

            if (response.admins.length > 0) {
                response.admins.forEach(function (element) {
                    memberList.append(`<li class=\"list-group-item d-flex justify-content-between align-items-start\">\
                            <div class=\"ms-2 me-auto\">\
                                <div class=\"fw-auto\">${element} <span class=\"badge ps-div-projects rounded-pill\">Admin</span></div>\
                                Member\
                            </div>\
                        </li>`);
                    console.log(`Admin ${element}`)
                });
            }

            if (response.members.length > 0) {
                response.members.forEach(function (element) {
                    memberList.append(`<li class=\"list-group-item d-flex justify-content-between align-items-start\">\
                            <div class=\"ms-2 me-auto\">\
                                <div class=\"fw-auto\">${element}</div>\
                                Member\
                            </div>\
                        </li>`);
                    console.log(`Member ${element}`)
                });
            }
        },
        async: true
    });
}