let projectId: number;
let archiveSpinner: JQuery<HTMLElement>;
let noArchivedCardsInfo: JQuery<HTMLElement>;

$(function () {
    projectId = $("#projectId").val() as number;
    archiveSpinner = $("#archiveSpinner") as JQuery<HTMLElement>;
    noArchivedCardsInfo = $("#noArchivedCardsInfo") as JQuery<HTMLElement>;

    $("#cardArchiveBtn").on("click", onShowArchive);
});

function onShowArchive(): void {
    archiveSpinner.removeClass("d-none");
    $.ajax({
        type: "GET",
        dataType: "json",
        url: `/api/Project/GetArchivedCards?projectId=${projectId}`,
        //beforeSend: function (request) {
        //    request.setRequestHeader("RequestVerificationToken", verificationToken);
        //},
        success: function (response) {
            archiveSpinner.addClass("d-none");
            response.cards.forEach(function (value) {
                console.log(value);
            });
        }
    });
}

