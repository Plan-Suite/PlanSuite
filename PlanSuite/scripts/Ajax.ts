import { data } from "jquery";

export class AjaxUtility {
    private static m_VerificationTokenElement: JQuery<HTMLElement>;
    private static m_VerificationTokenString: string;

    static {
        this.m_VerificationTokenElement = $("#RequestVerificationToken");

        if (this.m_VerificationTokenElement != null) {
            this.m_VerificationTokenString = this.m_VerificationTokenElement.val() as string;
        }
    }

    /**
     * Sends an async ajax request to the server
     * @param type Type of AJAX request, POST or GET
     * @param url URL of the request
     * @param json JSON data
     * @param onSuccess (optional) Function to call on success
     * @param onError (optional) Function to call on error
     * @param dataType (optional) Data type to expect back from server, by default is 'json'
     * @param contentType (optional) Data type to send to the server, by default is 'application/json'
     * @param processData (optional) Whether or not to process the data before sending, by default is true
     * @param cache (optional) Whether or not to cache the data, by default is true
     */
    private static SendAjax(type: string, url: string, json: any, onSuccess: any = null, onError: any = null, dataType: string = "json", contentType: string | false = 'application/json', processData: boolean = true, cache: boolean = true): void {
        $.ajax({
            type: type,
            dataType: dataType,
            contentType: contentType,
            cache: cache,
            processData: processData,
            url: url,
            beforeSend: function (request) {
                request.setRequestHeader("RequestVerificationToken", this.m_VerificationTokenString);
            },
            data: JSON.stringify(json),
            success: onSuccess,
            error: onError
        });
    }

    static SendPOST(url: string, json: any, onSuccess: any = null, onError: any = null, dataType: string = "json", contentType: string | false = 'application/json', processData: boolean = true, cache: boolean = true): void {
        AjaxUtility.SendAjax("POST", url, json, onSuccess, onError, dataType, contentType, processData, cache);
    }

    static SendGET(url: string, json: any, onSuccess: any = null, onError: any = null, dataType: string = "json", contentType: string | false = 'application/json', processData: boolean = true, cache: boolean = true): void {
        AjaxUtility.SendAjax("GET", url, json, onSuccess, onError, dataType, contentType, processData, cache);
    }
}