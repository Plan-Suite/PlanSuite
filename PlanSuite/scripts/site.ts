function isBlank(str): Boolean {
    return (!!!str || /^\s*$/.test(str));
}

function intToTier(tier: Number): String {
    let userTier: String = "Free";
    switch (tier) {
        case 1:
            userTier = "Plus";
            break;
        case 2:
            userTier = "Pro";
            break;
    }
    return userTier;
}

function arrayToString(array: Array<any>) {
    if (array.length < 1) {
        return "None";
    }

    let text = array.join(",");
    return text;
}