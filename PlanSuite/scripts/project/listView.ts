import { ProjectCommon } from "./projectCommon.js"

$(function () {
    $(".task-list-col").each(function () {
        $(this).children(".ps-task-item").each(function () {
            //console.log(`${$(this).attr("id")}`);
            let arr = $(this).attr("id").match(/[0-9]+$/);
            let id = parseInt(arr[0], 10);
            let dueTaskText = $(this).children(`#DueTaskText_${id}`).first();

            dueTaskText.on("click", function () {
                console.log(`view card ${id}`);
                ProjectCommon.viewCardButton(id);
            });
        });
    });

    $(".ps-add-task").on("click", function () {
        let arr = $(this).attr("id").match(/[0-9]+$/);
        let id = parseInt(arr[0], 10);
        console.log(id);
        $("#AddTask_ColumnId").val(id);
    });
});