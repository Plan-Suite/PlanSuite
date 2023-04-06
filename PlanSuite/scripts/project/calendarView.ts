/*import { Calendar } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import { ProjectCommon } from "./projectCommon.js";

$(function () {
    var projectId: number = $("#CalendarViewProjectId").val() as number;
    var calendarEl = document.getElementById('calendar');
    var calendar = new Calendar(calendarEl, {
        plugins: [dayGridPlugin],
        timeZone: 'UTC',
        initialView: 'dayGridMonth',
        firstDay: 1,
        events: `/api/Project/GetCalendarTasks?id=${projectId}`,
        eventClick: function (info) {
            console.log(
                'Title: ' + info.event.title + '\n' +
                'Id: ' + info.event.id
            );

            console.log(`view card ${info.event.id}`);
            ProjectCommon.viewCardButton(info.event.id);

            // Show card
            //$("#viewCardModal").modal('show');
        }
    });

    calendar.render();
});*/