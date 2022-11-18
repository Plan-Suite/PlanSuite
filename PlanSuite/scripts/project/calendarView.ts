enum Month {
    Jan = 1,
    Feb,
    Mar,
    Apr,
    May,
    Jun,
    Jul,
    Aug,
    Sep,
    Oct,
    Nov,
    Dec
}

function createCalendar(monthNameElement: JQuery<HTMLElement>, weekdaysElement: JQuery<HTMLElement>, daysElement: JQuery<HTMLElement>, year: number, month: Month) {
    console.log(`Creating calendar for ${month} ${year}`);
    daysElement.html("");

    var dateTime = new Date(year, month);

    // August<br><span style="font-size:18px">2021</span>
    monthNameElement.html(`${getMonthName(month)}<br><span style="font-size:18px">${year}</span>`);

    for (let i = 0; i < getDay(dateTime); i++) {
        console.log(`${i} = ${getDay(dateTime)}`);
        daysElement.append("<li></li>");
    }

    while (dateTime.getMonth() == month as number) {
        daysElement.append(`<li>${dateTime.getDate()}</li>`);

        if (getDay(dateTime) % 7 == 6) {
            daysElement.append(`<br>`);
        }

        dateTime.setDate(dateTime.getDate() + 1);
    }

    if (getDay(dateTime) != 0) {
        for (let i = getDay(dateTime); i < 7; i++) {
            daysElement.append("<li></li>");
        }
    }
}


function getDay(date) { // get day number from 0 (monday) to 6 (sunday)
    let day = date.getDay();
    if (day == 0) day = 7; // make Sunday (0) the last day
    return day - 1;
}

function getMonthName(month: Month): string {
    let monthName: string;
    switch (month) {
        default:
            monthName = "January";
            break;
        case Month.Feb:
            monthName = "Feburary";
            break;
        case Month.Mar:
            monthName = "March";
            break;
        case Month.Apr:
            monthName = "April";
            break;
        case Month.May:
            monthName = "May";
            break;
        case Month.Jun:
            monthName = "June";
            break;
        case Month.Jul:
            monthName = "July";
            break;
        case Month.Aug:
            monthName = "August";
            break;
        case Month.Sep:
            monthName = "September";
            break;
        case Month.Oct:
            monthName = "October";
            break;
        case Month.Nov:
            monthName = "November";
            break;
        case Month.Dec:
            monthName = "December";
            break;
    }
    return monthName;
}

$(function () {
    var monthName = $("#monthName");
    var weekdays = $(".weekdays");
    var days = $(".days");

    createCalendar(monthName, weekdays, days, 2022, Month.Oct);
});