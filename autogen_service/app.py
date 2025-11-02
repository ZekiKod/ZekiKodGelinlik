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

def get_stok_durumu(urun_adi: str) -> str:
    """
    Belirtilen ürün adının stok durumunu C# API'ından alır.
    """
    try:
        response = requests.get(f"http://localhost:5000/api/StokDurumu/{urun_adi}")
        if response.status_code == 200:
            return json.dumps(response.json())
        elif response.status_code == 404:
            return json.dumps({"error": "Ürün stokta bulunamadı."})
        else:
            return json.dumps({"error": f"API hatası: {response.status_code}"})
    except Exception as e:
        return json.dumps({"error": str(e)})

# Ajanların tanımlanması
assistant = autogen.AssistantAgent(
    name="Asistan",
    llm_config=llm_config,
    system_message="Sen bir sipariş ve stok takip asistanısın. Kullanıcının sorusunu analiz ederek sipariş durumu mu yoksa stok durumu mu sorduğunu anlarsın. Eğer siparişle ilgiliyse 'get_siparis_durumu' aracını, stokla ilgiliyse 'get_stok_durumu' aracını kullanırsın. Aldığın JSON sonucunu kullanıcıya anlamlı bir cümle ile özetlersin.",
)

user_proxy = autogen.UserProxyAgent(
    name="KullaniciTemsilcisi",
    human_input_mode="NEVER",
    max_consecutive_auto_reply=10,
    is_termination_msg=lambda x: x.get("content", "").rstrip().endswith("TERMINATE"),
    code_execution_config=False,
    llm_config=llm_config,
)

# Fonksiyonları ajana tanıtma
user_proxy.register_function(
    function_map={
        "get_siparis_durumu": get_siparis_durumu,
        "get_stok_durumu": get_stok_durumu
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
        message=f"Lütfen aşağıdaki soruyu yanıtlamak için uygun aracı kullan: '{soru}'",
    )

    # Son mesajı alıp döndürme
    yanit = user_proxy.last_message()["content"]
    return jsonify({"yanit": yanit})

if __name__ == "__main__":
    app.run(port=5001, debug=True)
