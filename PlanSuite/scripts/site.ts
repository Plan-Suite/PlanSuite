function isBlank(str): boolean {
    return (!!!str || /^\s*$/.test(str));
}

function intToTier(tier: number): string {
    let userTier: string = "Free";
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