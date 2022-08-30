export function isBlank(str): boolean {
    return (!!!str || /^\s*$/.test(str));
}

export function intToTier(tier: number): string {
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

export function arrayToString(array: Array<any>) {
    if (array.length < 1) {
        return "None";
    }

    let text = array.join(",");
    return text;
}

export function onDeleteInput(buttonName: string, comparisonValue: string, formName: string) {
    const button = $(`#${buttonName}`);
    const projName = $(`#${comparisonValue}`).val();
    const deleteProjConfirm = $(`#${formName}`);
    if (deleteProjConfirm.val() == projName) {
        button.removeAttr("disabled");
    }
}