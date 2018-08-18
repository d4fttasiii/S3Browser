"use strict";

(function () {

    // Downloading a file, converting the base64 string to blob a creating a dummy link
    window.downloadFile = (fileName, base64) => {
        var byteCharacters = atob(base64);
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], { type: "application/octet-stream" });

        var link = document.createElement('a');
        try {
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName;
            link.click();
        } catch (e) {
            console.error(e);
        }
        finally {
            link = undefined;
        }        
    };

    // Navigating with forcing a page reload
    window.navigateTo = (path) => {
        location.href = path;
    };

})();