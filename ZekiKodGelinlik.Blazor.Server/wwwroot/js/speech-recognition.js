window.startListening = function () {
    return new Promise((resolve, reject) => {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            reject("Taray�c� bu �zelli�i desteklemiyor.");
            return;
        }

        const recognition = new SpeechRecognition();
        recognition.lang = 'tr-TR'; // T�rk�e dil ayar�
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        recognition.start();

        recognition.onresult = function (event) {
            const command = event.results[0][0].transcript;
            resolve(command);
        };

        recognition.onerror = function (event) {
            reject("Hata olu�tu: " + event.error);
        };
    });
};
