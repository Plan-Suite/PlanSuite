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