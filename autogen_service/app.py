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

def get_satin_alma_onerisi(siparis_no: str) -> str:
    """
    Belirtilen sipariş numarası için satın alma önerilerini C# API'ından alır.
    """
    try:
        response = requests.get(f"http://localhost:5000/api/SatinAlmaOneri/{siparis_no}")
        if response.status_code == 200:
            return json.dumps(response.json())
        elif response.status_code == 404:
            return json.dumps({"error": "Sipariş bulunamadı."})
        else:
            return json.dumps({"error": f"API hatası: {response.status_code}"})
    except Exception as e:
        return json.dumps({"error": str(e)})

def create_satin_alma_talep(siparis_no: str, malzemeler: list) -> str:
    """
    Belirtilen sipariş numarası için verilen malzeme listesine göre satın alma talepleri oluşturur.
    """
    try:
        payload = {"SiparisNo": siparis_no, "Malzemeler": malzemeler}
        response = requests.post("http://localhost:5000/api/SatinAlmaTalepOlustur", json=payload)
        if response.status_code == 200:
            return json.dumps(response.json())
        else:
            return json.dumps({"error": f"API hatası: {response.status_code}", "details": response.text})
    except Exception as e:
        return json.dumps({"error": str(e)})

def get_aktif_operasyonlar() -> str:
    """
    Henüz tamamlanmamış tüm üretim operasyonlarının bir listesini C# API'ından alır.
    """
    try:
        response = requests.get("http://localhost:5000/api/UretimTakip/AktifOperasyonlar")
        return json.dumps(response.json()) if response.status_code == 200 else json.dumps({"error": f"API hatası: {response.status_code}"})
    except Exception as e:
        return json.dumps({"error": str(e)})

def update_operasyon_durumu(operasyon_id: int, yeni_durum: str) -> str:
    """
    Belirtilen operasyonun durumunu günceller.
    """
    try:
        payload = {"OperasyonId": operasyon_id, "YeniDurum": yeni_durum}
        response = requests.post("http://localhost:5000/api/UretimTakip/OperasyonDurumGuncelle", json=payload)
        return json.dumps(response.json()) if response.status_code == 200 else json.dumps({"error": f"API hatası: {response.status_code}", "details": response.text})
    except Exception as e:
        return json.dumps({"error": str(e)})

def send_bildirim(kime: str, mesaj: str) -> str:
    """
    Belirtilen kişiye veya departmana bir bildirim gönderir.
    """
    try:
        payload = {"Kime": kime, "Mesaj": mesaj}
        response = requests.post("http://localhost:5000/api/UretimTakip/BildirimGonder", json=payload)
        return json.dumps(response.json()) if response.status_code == 200 else json.dumps({"error": f"API hatası: {response.status_code}", "details": response.text})
    except Exception as e:
        return json.dumps({"error": str(e)})

def get_model_resim(model_no: str) -> str:
    """
    Belirtilen model numarasına ait resmin URL'sini C# API'ından alır.
    """
    try:
        response = requests.get(f"http://localhost:5000/api/ModelResim/{model_no}")
        return json.dumps(response.json()) if response.status_code == 200 else json.dumps({"error": f"API hatası: {response.status_code}"})
    except Exception as e:
        return json.dumps({"error": str(e)})

# Ajanların tanımlanması
assistant = autogen.AssistantAgent(
    name="Asistan",
    llm_config=llm_config,
    system_message="""Sen proaktif bir sipariş, stok ve satın alma asistanısın.
Kullanıcının sorusunu analiz ederek doğru aracı ('get_siparis_durumu', 'get_stok_durumu', 'get_satin_alma_onerisi', 'get_model_resim') kullanırsın.
Tüm yanıtlarını MUTLAKA aşağıdaki JSON formatında vermelisin:
{
  "response_type": "text | material_list | image",
  "data": "yanıt metni | malzeme listesi (JSON) | resim URL'si"
}
Örneğin, 'get_model_resim' aracını kullandıktan sonra, yanıtın şöyle olmalı:
{"response_type": "image", "data": {"imageUrl": "KumasResim/IPEKBRODE-123.jpg"}}
'get_satin_alma_onerisi' aracını kullandığında, yanıtın şöyle olmalı:
{"response_type": "material_list", "data": [{"MalzemeTuru": "Kumaş", "MalzemeAdi": "İpek", "Miktar": 10.0, "Birim": "Metre"}]}
Diğer tüm metin bazlı yanıtlar için 'text' response_type'ını kullan.
Satın alma önerilerini sunduktan sonra, proaktif olarak 'Bu talepleri oluşturayım mı?' diye sor. Kullanıcı onaylarsa, 'create_satin_alma_talep' aracını çalıştır.""",
)

ustasi_agent = autogen.AssistantAgent(
    name="UstasiAgent",
    llm_config=llm_config,
    system_message="Sen otonom bir üretim takip ustasısın. Görevin, 'get_aktif_operasyonlar' aracını kullanarak tüm aktif üretim operasyonlarını kontrol etmektir. Tamamlanmış bir operasyondan sonraki sıradaki operasyonları bulur, durumlarını 'Beklemede' olarak ayarlamak için 'update_operasyon_durumu' aracını kullanırsın. Eğer bir sonraki adım fason işlemi ise, 'send_bildirim' aracını kullanarak ilgili departmanı bilgilendirirsin. Ayrıca, başlaması gerekip de başlamamış gecikmiş operasyonları tespit eder ve yöneticilere bildirirsin. Tüm bu adımları kendi kendine, proaktif olarak gerçekleştirirsin.",
)

user_proxy = autogen.UserProxyAgent(
    name="KullaniciTemsilcisi",
    human_input_mode="NEVER",
    max_consecutive_auto_reply=10,
    is_termination_msg=lambda x: x.get("content", "").rstrip().endswith("TERMINATE"),
    code_execution_config=False,
    llm_config=llm_config,
    system_message="Bir yönetici. Sohbeti başlatır, görevi açıklar ve ajanların sonuçlarını özetler. 'Tüm üretimi kontrol et' komutu aldığında, bu görevi 'UstasiAgent'a devretmelisin.",
)

# Fonksiyonları ajana tanıtma
user_proxy.register_function(
    function_map={
        "get_siparis_durumu": get_siparis_durumu,
        "get_stok_durumu": get_stok_durumu,
        "get_satin_alma_onerisi": get_satin_alma_onerisi,
        "create_satin_alma_talep": create_satin_alma_talep,
        "get_aktif_operasyonlar": get_aktif_operasyonlar,
        "update_operasyon_durumu": update_operasyon_durumu,
        "send_bildirim": send_bildirim,
        "get_model_resim": get_model_resim
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

import threading
import time

def background_worker():
    """
    Arka planda periyodik olarak UstasiAgent'ı tetikler.
    """
    while True:
        print("Üretim takip ajanı periyodik kontrolü başlatıyor...")
        try:
            user_proxy.initiate_chat(
                ustasi_agent,
                message="Tüm üretimi kontrol et ve gerekeni yap.",
            )
            print("Periyodik kontrol tamamlandı.")
        except Exception as e:
            print(f"Arka plan görevinde hata oluştu: {e}")

        # 5 dakika bekle
        time.sleep(300)

if __name__ == "__main__":
    # Arka plan görevini bir thread olarak başlat
    worker_thread = threading.Thread(target=background_worker, daemon=True)
    worker_thread.start()

    # Flask uygulamasını çalıştır
    app.run(port=5001, debug=True)
