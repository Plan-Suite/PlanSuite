import $ from "jquery";

var firstName: JQuery<HTMLElement> = $("#Input_FirstName");
var lastName: JQuery<HTMLElement> = $("#Input_LastName");
var password: JQuery<HTMLElement> = $("#Input_Password");
var confirmPassword: JQuery<HTMLElement> = $("#Input_ConfirmPassword");
var continueBtn: JQuery<HTMLElement> = $("#continueBtn");
var backBtn: JQuery<HTMLElement> = $("#backBtn");

var part1: JQuery<HTMLElement> = $("#part_1");
var part2: JQuery<HTMLElement> = $("#part_2");
var error: JQuery<HTMLElement> = $("#error");

continueBtn.on("click", function () {
    if (firstName.text().length > 0 && lastName.text().length > 0) {
        part1.addClass("d-none");
        part2.removeClass("d-none");
        error.addClass("d-none");
    }
    else {
        error.removeClass("d-none");
        error.text("You must input both your first name and last name.");
    }
});

$("#backBtn").on("click", function () {
    part2.addClass("d-none");
    part1.removeClass("d-none");
    error.addClass("d-none");
});