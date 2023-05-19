import { ProjectCommon } from "./projectCommon"

$(function () {
    $(".progress-list-col").each(function () {
        console.log(`list col ${$(this).attr('class')}`);
        $(this).children(".table").each(function () {
            console.log(`item ${$(this).attr('class')}`);
            $(this).children("tbody").each(function () {
                console.log(`item ${$(this).attr('class')}`);
                $(this).children(".progress-task-item").each(function () {
                    console.log(`item ${$(this).attr('class')}`);
                    let arr = $(this).attr("id").match(/[0-9]+$/);
                    let id = parseInt(arr[0], 10);

                    $(this).on("click", function () {
                        console.log(`view card ${id}`);
                        ProjectCommon.viewCardButton(id);
                    });
                });
            });
        });
    });

    $(".progress-add-task").on("click", function () {
        let arr = $(this).attr("id").match(/[0-9]+$/);
        let id = parseInt(arr[0], 10);
        console.log(id);
        $("#AddTask_ColumnId").val(id);
    });
});