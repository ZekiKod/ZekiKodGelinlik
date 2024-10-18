using Microsoft.JSInterop;
using ZekiKodGelinlik.Module.Controllers;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    public static class CommandProcessor
    {
        private static SpeechCommandController _speechCommandController;

        // Bağımlılık enjeksiyonu için başlatma metodu
        public static void Initialize(SpeechCommandController speechCommandController)
        {
            _speechCommandController = speechCommandController;
        }

        // JavaScript tarafından çağrılabilir metot
        [JSInvokable("ProcessCommand")]
        public static async Task<string> ProcessCommand(string commandText)
        {
            if (_speechCommandController == null)
            {
                return "SpeechCommandController henüz başlatılmadı.";
            }

            var result = await _speechCommandController.ProcessCommand(commandText);
            return result ?? "Komut işlenemedi.";
        }
    }
}
