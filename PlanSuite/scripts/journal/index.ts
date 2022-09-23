$(document).ready(function () {
    $("#notesHolder").children().each(function (i) {
        $(this).on("click", function () { window.location.replace(`/Journal/Write?id=${$(this).attr("id")}`); });
    })
});
