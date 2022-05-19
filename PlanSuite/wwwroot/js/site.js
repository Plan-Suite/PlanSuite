// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    $("#sortable").sortable();
    $("#sortable").disableSelection();
})

function isBlank(str) {
    return (!!!str || /^\s*$/.test(str));
}

function intToTier(tier) {
    var tier = "Free";
    switch (tier) {
        case 1:
            tier = "Plus";
            break;
        case 2:
            tier = "Pro";
            break;
    }
    return tier;
}

function arrayToString(array) {
    if (array.length < 1) {
        return "None";
    }

    let text = array.join(",");
    return text;
}