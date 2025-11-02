import requests
import json
from semantic_kernel.skill_definition import sk_function, sk_function_context_parameter

class FiyatTeklifiPlugin:
    @sk_function(
        description="Bir modelin onaylanmış maliyetini TL olarak döndürür.",
        name="get_model_maliyet",
    )
    @sk_function_context_parameter(
        name="model_no",
        description="Maliyeti alınacak modelin numarası",
    )
    def get_model_maliyet(self, model_no: str) -> str:
        try:
            response = requests.get(f"http://localhost:5000/api/FiyatTeklifi/ModelMaliyet/{model_no}")
            return json.dumps(response.json()) if response.status_code == 200 else f"Hata: {response.status_code}"
        except Exception as e:
            return f"Hata: {str(e)}"

    @sk_function(
        description="İstenen para biriminin güncel kurunu döndürür.",
        name="get_doviz_kuru",
    )
    @sk_function_context_parameter(
        name="kur",
        description="Kuru alınacak para birimi (örn: USD, EUR)",
    )
    def get_doviz_kuru(self, kur: str) -> str:
        try:
            response = requests.get(f"http://localhost:5000/api/FiyatTeklifi/DovizKuru/{kur}")
            return json.dumps(response.json()) if response.status_code == 200 else f"Hata: {response.status_code}"
        except Exception as e:
            return f"Hata: {str(e)}"
