using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.ExpressApp;
using Sirketiz.Module.BusinessObjects.Sirket_izDB;
using ZekiKodGelinlik.Module.BusinessObjects;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class ProvaDetay
    {
        public ProvaDetay(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction();

            ApplicationUser currentUser = (ApplicationUser)SecuritySystem.CurrentUser;
            if (currentUser != null && currentUser.kisi_kartlari_to != null)
            {
                // currentUser.kisi_kartlari_to nesnesini geçerli oturuma aktar
                kisi_kartlari kisiKartlariToInCurrentSession = Session.GetObjectByKey<kisi_kartlari>(currentUser.kisi_kartlari_to.Oid);
                Temsilci = kisiKartlariToInCurrentSession;
            }
        }

        protected override void OnSaving()
        {
            base.OnSaving();

            // Eğer bir operasyon seçilmişse, prova notlarını o operasyonun açıklamasına ekle
            if (IlgiliOperasyon != null)
            {
                string provaBilgisi = $"--- Prova Notu ({DateTime.Now:dd.MM.yyyy HH:mm}) ---\n";
                if (!string.IsNullOrEmpty(YapilanDuzeltmeler))
                {
                    provaBilgisi += $"Yapılan Düzeltmeler: {YapilanDuzeltmeler}\n";
                }
                if (!string.IsNullOrEmpty(ProvaNotlari))
                {
                    provaBilgisi += $"Ek Notlar: {ProvaNotlari}\n";
                }

                // Mevcut açıklamaya ekleme yap, üzerine yazma
                IlgiliOperasyon.Aciklama += "\n" + provaBilgisi;

                // Opsiyonel: Operasyonun durumunu "Beklemede" veya "Devam Ediyor" gibi bir duruma getirebiliriz
                // Örnek: IlgiliOperasyon.Durum = "Prova Değişikliği Bekliyor";
            }
        }
    }

}
