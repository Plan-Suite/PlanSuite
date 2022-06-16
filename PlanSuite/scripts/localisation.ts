import { getCookie } from './cookie.js'

interface LocalisationResponse {
    data: Record<string, string>;
}

export class Localisation {
    language: string
    strings: Map<string, string>;
    ready: boolean = false;
    readonly URL: string = "/api/Localisation/GetStrings?userLang=";

    constructor() {
        this.language = getCookie("user_lang");
        this.strings = new Map();
        const ajaxSettings = {
            type: "GET",
            dataType: "json",
            contentType: "application/json",
            url: this.URL + this.language,
            timeout: 3000
        }
        $.ajax(ajaxSettings)
            .done((response: LocalisationResponse) => {
                console.log(response.data);
                Object.keys(response.data).map(record => {
                    this.strings.set(record, response.data[record]);
                });
            });
    }

    public Get(key: string): string {
        return this.strings.get(key);
    }
}