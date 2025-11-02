from flask import Flask, request, jsonify
import autogen
import requests
import json
import os

app = Flask(__name__)

# AutoGen yapılandırması
api_key = os.environ.get("OPENAI_API_KEY")
if not api_key:
    raise ValueError("OPENAI_API_KEY ortam değişkeni ayarlanmamış.")

config_list = [
    {
        "model": "gpt-3.5-turbo",
        "api_key": api_key,
    }
]

llm_config = {
    "config_list": config_list,
    "cache_seed": 42,
}

# C# API'ını çağıran "araç" fonksiyonu
def get_siparis_durumu(siparis_no: str) -> str:
    """
    Belirtilen sipariş numarasının durumunu ve operasyon detaylarını C# API'ından alır.
    """
    try:
        response = requests.get(f"http://localhost:5000/api/SiparisDurumu/{siparis_no}")
        if response.status_code == 200:
            return json.dumps(response.json())
        elif response.status_code == 404:
            return json.dumps({"error": "Sipariş bulunamadı."})
        else:
            return json.dumps({"error": f"API hatası: {response.status_code}"})
    except Exception as e:
        return json.dumps({"error": str(e)})

# Ajanların tanımlanması
assistant = autogen.AssistantAgent(
    name="SiparisAsistani",
    llm_config=llm_config,
    system_message="Sen bir sipariş takip asistanısın. Kullanıcının sorduğu siparişin durumunu özetlersin. get_siparis_durumu aracını kullanarak sipariş detaylarını almalı ve sonucu kullanıcıya anlamlı bir cümle ile sunmalısın.",
)

user_proxy = autogen.UserProxyAgent(
    name="KullaniciTemsilcisi",
    human_input_mode="NEVER",
    max_consecutive_auto_reply=10,
    is_termination_msg=lambda x: x.get("content", "").rstrip().endswith("TERMINATE"),
    code_execution_config=False,
    llm_config=llm_config,
)

# Fonksiyonu ajana tanıtma
user_proxy.register_function(
    function_map={
        "get_siparis_durumu": get_siparis_durumu
    }
)


@app.route("/soru", methods=["POST"])
def soru_sor():
    data = request.get_json()
    soru = data.get("soru")

    if not soru:
        return jsonify({"error": "Soru alanı boş olamaz."}), 400

    # AutoGen sohbetini başlatma
    user_proxy.initiate_chat(
        assistant,
        message=f"Lütfen '{soru}' sorusunu analiz et, sipariş numarasını bul ve get_siparis_durumu aracını kullanarak bu siparişin durumunu öğren. Sonucu bana özetle.",
    )

    # Son mesajı alıp döndürme
    yanit = user_proxy.last_message()["content"]
    return jsonify({"yanit": yanit})

if __name__ == "__main__":
    app.run(port=5001, debug=True)
