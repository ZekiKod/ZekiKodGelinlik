window.startListening = function () {
    return new Promise((resolve, reject) => {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            reject("Tarayýcý bu özelliði desteklemiyor.");
            return;
        }

        const recognition = new SpeechRecognition();
        recognition.lang = 'tr-TR'; // Türkçe dil ayarý
        recognition.interimResults = false;
        recognition.maxAlternatives = 1;

        recognition.start();

        recognition.onresult = function (event) {
            const command = event.results[0][0].transcript;
            resolve(command);
        };

        recognition.onerror = function (event) {
            reject("Hata oluþtu: " + event.error);
        };
    });
};
