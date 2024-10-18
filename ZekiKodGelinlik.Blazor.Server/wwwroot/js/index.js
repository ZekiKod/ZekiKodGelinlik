
// Camera states
var states = {
    OFF: 1,
    CAPTURING: 2,
    STOPPED: 3,
    RESETTING: 4,
    CLOSING: 5
}

var state = states.OFF;

var gallery = [];
var videoPlayers = [];
var codeReader = new ZXing.BrowserMultiFormatReader();
console.log('ZXing code reader initialized');

var deferredPrompt;
var dotnetHelper = null;

// ---------- -------------------------  ---------- //


// Sayfayı yenileyen fonksiyon burada yer almalı
window.JSFunctions.refreshPage = function () {
    console.log("Refreshing the page...");
    window.location.reload();  // Sayfayı yenile
};
    //
    //
    SearchRecord: function (barcode) {
        if (dotnetHelper != null)
            return dotnetHelper.invokeMethodAsync('SearchRecordCaller', { barcode });
        return null;
    },

    //
    // Start streaming from cameras
    //
    StartCamStreaming: function (param) {

        dotnetHelper = param;

        console.log(codeReader);
        $("#scanStatus").empty();

        if (gallery) {
            console.log('Emptied previous gallery');
            gallery = [];
        }

        state = states.OFF;
        console.log('State -> OFF');

        if (state == states.OFF) {
            window.JSFunctions.InitializeMedia();
        }

    },

    //
    // 
    //
    CloseCaptureImageModal: function () {

        state = states.CLOSING;
        console.log('State -> CLOSING');

        if (codeReader) {
            codeReader.reset();
            console.log('CodeReader resetted.')
        }
        else {
            console.log('CodeReader reset impossible because is undefined .')
        }

        $("#gallery").dxGallery("dispose");
        $("#gallery").empty();
        gallery = [];
        videoPlayers = [];
        state = states.OFF;
        console.log('State -> OFF');

    },

    //
    // Inizializzazione media
    //
    InitializeMedia: function () {

        //state = states.CAPTURING;
        //console.log('State -> CAPTURING');

        if (!('mediaDevices' in navigator)) {
            console.log("!'mediaDevices' in navigator");
            navigator.mediaDevices = {};
        }

        if (!('getUserMedia' in navigator.mediaDevices)) {
            console.log("!'getUserMedia' in navigator.mediaDevices");
            navigator.mediaDevices.getUserMedia = function (constraints) {
                var getUserMedia = navigator.webkitGetUserMedia || navigator.mozGetUserMedia;

                if (!getUserMedia) {
                    return Promise.reject(new Error('getUserMedia is not implemented!'));
                }

                return new Promise(function (resolve, reject) {
                    getUserMedia.call(navigator, constraints, resolve, reject);
                });
            }
        }

        navigator.mediaDevices.enumerateDevices().then(function (devices) {

            var isAndroid = false;

            //For android phones
            console.log("devices list (Android)");
            for (var i = 0; i < devices.length; i++) {
                console.log(JSON.stringify(devices[i]));
                if (devices[i].kind === "videoinput" && devices[i].label.includes("back")) {
                    //if (devices[i].label.includes("0")) {
                    console.log(devices[i]);
                    gallery.push(devices[i]);
                    isAndroid = true;
                    // }
                }
            }

            //For Iphones
            if (isAndroid === false) {
                console.log("devices list (Apple)");
                navigator.mediaDevices.enumerateDevices().then(videoInputDevices => {
                    videoInputDevices.forEach(device =>
                        console.log(JSON.stringify(device))
                    );
                })
                for (var i = 0; i < devices.length; i++) {
                    console.log(devices[i]);
                    //window.JSFunctions.ShowScanStatus(devices[i].deviceId + " | " + devices[i].kind + " | " + devices[i].label);
                    //if (devices[i].kind === "videoinput" && (devices[i].label.includes("back") || devices[i].label.includes("posteriore")) ) {
                    console.log(devices[i]);
                    gallery.push(devices[i]);
                    //}
                }
            }

            console.log("Gallery initialization");

            $("#gallery").dxGallery({
                dataSource: gallery,
                height: '80%',
                width: '100%',
                loop: true,
                slideshowDelay: 105000,
                showNavButtons: true,
                showIndicator: true,
                selectedIndex: 0,
                // gestione template della gallery
                itemTemplate: function (item, index) {

                    console.log("Gallery Item template!");
                    console.log("Item:" + index);
                    console.log(item);

                    var result = $("<div>");
                    $("<video>").attr({ id: "player-" + item.deviceId + "", autoplay: true }).attr("width", "100%").attr("height", "100%").appendTo(result);
                    $("<canvas>").attr("id", "canvas-" + item.deviceId + "").attr("width", "100%").attr("height", "100%").appendTo(result);
                    return result;
                },
                // 
                onItemRendered: function (e) {
                    if (e.itemIndex == 0) {
                        console.log("element rendered:");
                        console.log(e);

                        navigator.mediaDevices.getUserMedia(
                            {
                                video: {
                                    width: { exact: 640 },
                                    height: { exact: 640 },
                                    deviceId: e.itemData.deviceId
                                }
                            })
                            .then(function (stream) {
                                document.querySelector('#player-' + e.itemData.deviceId).srcObject = stream;
                                document.querySelector('#player-' + e.itemData.deviceId).style.display = 'block';
                            })
                            .catch(function (err) {
                                //imagePickerArea.style.display = 'block';
                                console.log("NO CAMERA!: " + err);
                            });

                        window.JSFunctions.InitializeScanner(e.itemData.deviceId);
                    }
                },
                onSelectionChanged: function (e) {

                    console.log("elements:");
                    console.log(e);

                    console.log("element removed:");
                    console.log(e.removedItems);

                    var videoPlayer = document.querySelector('#player-' + e.removedItems[0].deviceId);

                    if (videoPlayer.srcObject) {
                        videoPlayer.srcObject.getVideoTracks().forEach(function (track) {
                            track.stop();
                        });
                    }

                    if (codeReader) {
                        codeReader.reset();
                        console.log('CodeReader resetted.')
                    }
                    else {
                        console.log('CodeReader reset impossible because is undefined .')
                    }

                    console.log("element added:");
                    console.log(e.addedItems);

                    navigator.mediaDevices.getUserMedia(
                        {
                            video: {
                                width: { exact: 640 },
                                height: { exact: 640 },
                                deviceId: e.addedItems[0].deviceId
                            }
                        })
                        .then(function (stream) {
                            document.querySelector('#player-' + e.addedItems[0].deviceId).srcObject = stream;
                            document.querySelector('#player-' + e.addedItems[0].deviceId).style.display = 'block';
                        })
                        .catch(function (err) {
                            //imagePickerArea.style.display = 'block';
                            console.log("NO CAMERA!")
                        });

                    window.JSFunctions.InitializeScanner(e.addedItems[0].deviceId);

                }
            });
        });

    },

    //
    // 
    //
    StopCapture: function () {
        videoPlayers.forEach(function (videoPlayer) {
            if (videoPlayer.srcObject != null)
                videoPlayer.srcObject.getVideoTracks().forEach(function (track) { track.stop(); });
        });
        videoPlayers = [];
        console.log("VideoPlayers array emptied");
        window.JSFunctions.CloseCaptureImageModal();

    },

    //
    //
    //
    InitializeScanner: function (deviceId) {

        console.log("element initialized id:");
        console.log(deviceId);
        console.log("CodeReader:");
        console.log(codeReader);

        var videoPlayer = document.querySelector('#player-' + deviceId);

        var videoSourceSelect = document.getElementById('videoSourceSelect');
        while (videoSourceSelect.firstChild) {
            videoSourceSelect.removeChild(videoSourceSelect.firstChild);
        }

        codeReader.getVideoInputDevices()
            .then((videoInputDevices) => {
                if (videoInputDevices.length >= 1) {
                    videoInputDevices.forEach((element) => {
                        const date = new Date();
                        const sourceOption = document.createElement('option')
                        sourceOption.text = element.label + " | " + date.getTime();
                        sourceOption.value = element.deviceId
                        videoSourceSelect.appendChild(sourceOption)
                    })
                    const sourceElem = document.querySelector('#player-' + deviceId);
                    console.log("Source element:");
                    console.log(sourceElem)
                    // Stream decode
                    codeReader.decodeFromStream(videoPlayer.srcObject, 'player-' + deviceId, (result, err) => {
                        if (result) {
                            window.JSFunctions.StopCapture();
                            var barcode = result.text;
                            if (barcode.length > 12 && barcode.startsWith("0"))
                                barcode = barcode.substring(1);
                            console.log("Capture stopped");
                            console.log("Barcode read:");
                            console.log(barcode);
                            return window.JSFunctions.SearchRecord(barcode);
                        }
                        if (err && !(err instanceof ZXing.NotFoundException)) {
                            console.log(err);
                        }
                    });
                    console.log(`Started continous decode from camera with id ${deviceId}`)
                    videoPlayers.push(videoPlayer);
                }
            });

    },

    //
    // Function that show if the product was not found
    //
    ShowScanStatus: function (message) {
        const statusOption = document.createElement('option')
        statusOption.text = message;
        statusOption.value = "status";
        var scanStatus = document.getElementById('scanStatus')
        scanStatus.appendChild(statusOption)
    },


}

