//import { * } from "../../lib/summernote/dist/js/summernote.min.js";
import * as summernote from 'summernote';

$(document).ready(function () {

    $('#JournalEntryUpdate_JournalId').val("");

    $('#JournalEntryUpdate_Content').summernote({
        placeholder: "Content",
        tabsize: 2,
        height: 150,
        toolbar: [
            ['style', ['bold', 'italic', 'underline', 'clear']],
            ['font', ['strikethrough', 'superscript', 'subscript']],
            ['fontsize', ['fontsize']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['height', ['height']]
        ],
        callbacks: {
            onImageUpload: function (files) {
                for (let i = 0; i < files.length; i++) {
                    UploadImage(files[i]);
                }
            }/*,
            onChange: function (contents, $editable) {
                console.log(`Contents: ${contents}`);
                console.log(`$editable: ${$editable}`);
            }*/
        }
    });

    $("#JournalEntryUpdate_Content").on("summernote.media.delete", function (we, e) {
        DeleteImage(e[0].src);
    });
});

function UploadImage(file) {
    var url = "/Journal/UploadImage";

    var formData = new FormData();
    formData.append("file", file);
    $.ajax({
        type: "POST",
        url: url,
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (FileUrl) {
            var imgNode = document.createElement("img");
            imgNode.src = `/uploaded_images/${FileUrl.fileUrl}`;
            console.log(imgNode.src);
            $("#JournalEntryUpdate_Content").summernote("insertNode", imgNode);
        },
        error: function (data) {
            console.error(data.responseText);
        }
    });
} 

function DeleteImage(file) {
    var url = "/Journal/DeleteImage";
    console.log("DeleteFile: "+file);

    var formData = new FormData();
    formData.append("file", file);

    console.log(formData);
    $.ajax({
        type: "POST",
        url: url,
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        success: function (data) {
            console.log(data);
        },
        error: function (data) {
            console.error(data.responseText);
        }
    });
} 