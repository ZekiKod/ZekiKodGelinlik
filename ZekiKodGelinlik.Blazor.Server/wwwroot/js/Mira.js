let recognition;
let isListening = false;
let commandActive = false;
let uniqueID = "";
let selectedCustomerNumber = null;
let userModifiedText = ''; // Elle düzenlenmiş metin
let finalText = ''; // Tamamlanan nihai metin

// Yeni eklenen değişkenler
let sequenceNumber = 1;
let representativeName = "Temsilci Adı"; // Temsilcinin adı
let orderSlipNumber = 152; // Sipariş föy numarası
let pendingSelection = false; // Seçim yapılıyor mu?

// Unique ID oluşturma
function generateUniqueID() {
    return 'speechTextBox_' + Math.random().toString(36).substring(2, 11);
}

// Unique ID atama ve metin kutusunu işleme
function assignUniqueID() {
    uniqueID = generateUniqueID();
    let inputElement = document.querySelector("textarea");
    if (inputElement) {
        inputElement.id = uniqueID;
        inputElement.classList.add('red-border');
        console.log("Atanan benzersiz ID: " + uniqueID);

        // Metin kutusunun `input` olayını dinleyin
        inputElement.addEventListener('input', function () {
            userModifiedText = inputElement.value;
            finalText = userModifiedText; // Elle düzenlenen metin nihai metin olarak alınır
        });
    }
}

// Komuta göre metin kutusunun sınırlarını güncelleme
function updateBorderBasedOnCommand() {
    let inputElement = document.getElementById(uniqueID);
    if (inputElement) {
        if (commandActive) {
            inputElement.classList.remove('red-border');
            inputElement.classList.add('green-border');
        } else {
            inputElement.classList.remove('green-border');
            inputElement.classList.add('red-border');
        }
    }
}

// Unique ID'yi temizleme
function clearUniqueID() {
    let inputElement = document.getElementById(uniqueID);
    if (inputElement) {
        inputElement.id = "";
        console.log("ID temizlendi.");
    }
}

// Ses tanıma ve komut dinleme işlevini başlatma
function startMiraListening() {
    if (window.hasOwnProperty('webkitSpeechRecognition') && !isListening) {
        recognition = new webkitSpeechRecognition();
        recognition.continuous = true;
        recognition.interimResults = true;
        recognition.lang = 'tr-TR';

        recognition.onstart = function () {
            console.log("Mikrofon açıldı ve dinlemeye başladı.");
            // Mikrofon simgesini güncelle
            document.getElementById('microphone-icon').classList.remove('fa-microphone');
            document.getElementById('microphone-icon').classList.add('fa-microphone-slash');
        };

        recognition.onresult = function (event) {
            let interimTranscript = ''; // Geçici metin

            for (let i = event.resultIndex; i < event.results.length; ++i) {
                if (!event.results[i].isFinal) {
                    interimTranscript += event.results[i][0].transcript;
                }
            }

            // Tanınan metni küçük harfe çevir
            let transcript = event.results[event.resultIndex][0].transcript.toLowerCase();
            console.log("Geçici metin: " + transcript);

            // 'hey mira' komutunu kontrol et
            if (!commandActive && transcript.includes("hey mira")) {
                commandActive = true;
                console.log("Hey Mira komutu algılandı, şimdi kaydetmeye başlayacak...");
                updateBorderBasedOnCommand();
                announceResult("Seni dinliyorum");
                return;
            }

            // 'mira bekle' komutunu kontrol et
            if (transcript.includes("mira bekle")) {
                commandActive = false;
                updateBorderBasedOnCommand();
                announceResult("Dinlemeyi duraklattım.");
                logCommand("mira bekle");
                return;
            }

            // 'mira devam et' komutunu kontrol et
            if (transcript.includes("mira devam et")) {
                commandActive = true;
                isListening = true;
                updateBorderBasedOnCommand();
                announceResult("Dinlemeye devam ediyorum.");
                logCommand("mira devam et");
                return;
            }

            if (commandActive && !pendingSelection) {
                let inputElement = document.getElementById(uniqueID);
                if (inputElement) {
                    let newFinalText = ''; // Nihai yeni tanınan metin
                    for (let i = event.resultIndex; i < event.results.length; ++i) {
                        if (event.results[i].isFinal) {
                            newFinalText += event.results[i][0].transcript.toLowerCase();
                        }
                    }

                    finalText = userModifiedText + " " + newFinalText; // Elle düzenlenmiş metin üzerine yeni metni ekle
                    inputElement.value = finalText; // Nihai metni textarea'ya ekle
                    userModifiedText = finalText;

                    // 'mira kaydet' komutunu kontrol et
                    if (newFinalText.includes("mira kaydet")) {
                        commandActive = false;
                        sendToCSharp(inputElement.value); // Metin kutusunun tamamını gönder
                        updateBorderBasedOnCommand();
                        announceResult("Komutu aldım, sonucu döneceğim.");
                        // Mikrofonu duraklatıp tekrar başlatabiliriz
                        recognition.stop();
                        return;
                    }
                }
            }
        };

        recognition.onerror = function (event) {
            console.error("Hata oluştu: ", event.error);
            // Eğer hata 'aborted' değilse, mikrofonu yeniden başlat
            if (event.error !== 'aborted') {
                setTimeout(function() {
                    if (!isListening) {
                        startMiraListening();
                    }
                }, 1000);
            }
        };

        recognition.onend = function () {
            console.log("Mikrofon kapandı.");
            isListening = false;
            commandActive = false;
            updateBorderBasedOnCommand();
            // Mikrofon simgesini güncelle
            document.getElementById('microphone-icon').classList.remove('fa-microphone-slash');
            document.getElementById('microphone-icon').classList.add('fa-microphone');
            // Mikrofonu kısa bir gecikme ile yeniden başlat
            setTimeout(function() {
                if (!isListening) {
                    startMiraListening();
                }
            }, 1000); // 1 saniye gecikme
        };

        recognition.start();
        isListening = true;
        commandActive = true;
        updateBorderBasedOnCommand();
    } else {
        if (isListening) {
            console.log("Mikrofon zaten aktif.");
        } else {
            console.error("Tarayıcınız ses tanıma özelliğini desteklemiyor.");
        }
    }
}

// Mikrofonu durdurma fonksiyonu
function stopMiraListening() {
    if (recognition && isListening) {
        recognition.stop();
        isListening = false;
        commandActive = false;
        updateBorderBasedOnCommand();
        console.log("Mikrofon durduruldu.");
        // Mikrofon simgesini güncelle
        document.getElementById('microphone-icon').classList.remove('fa-microphone-slash');
        document.getElementById('microphone-icon').classList.add('fa-microphone');
    }
}

// C#'a komut gönderme
async function sendToCSharp(commandText) {
    console.log("C#'a komut gönderiliyor: " + commandText);
    try {
        await DotNet.invokeMethodAsync('ZekiKodGelinlik.Blazor.Server', 'StaticProcessCommand', commandText);
        console.log("Komut başarıyla C#'a gönderildi: " + commandText);
    } catch (error) {
        console.error("C#'a komut gönderilirken hata oluştu:", error);
    }
}

// Sayfa yüklendiğinde
window.onload = function () {
    assignUniqueID();
    startMiraListening(); // Mikrofonu otomatik olarak başlat
}

window.onbeforeunload = function () {
    clearUniqueID();
}

// Mikrofon butonuna tıklama etkinliği
document.getElementById('microphone-button').addEventListener('click', function() {
    if (isListening) {
        // Mikrofon aktifse, durdur
        stopMiraListening();
    } else {
        // Mikrofon kapalıysa, başlat
        startMiraListening();
    }
});

// 'Kaydet' butonuna tıklama etkinliği
document.getElementById('save-button').addEventListener('click', function() {
    let inputElement = document.getElementById(uniqueID);
    if (inputElement) {
        commandActive = false;
        sendToCSharp(inputElement.value); // Metin kutusunun tamamını gönder
        updateBorderBasedOnCommand();
        announceResult("Komutu aldım, sonucu döneceğim.");
        // Mikrofonu duraklatıp tekrar başlatabiliriz
        recognition.stop();
    }
});
