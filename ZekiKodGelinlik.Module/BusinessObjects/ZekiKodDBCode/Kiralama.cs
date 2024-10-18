using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using ZekiKodGelinlik.Module;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class Kiralama_Satis
    {
        public Kiralama_Satis(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }


        protected override void OnSaved()
        {
            base.OnSaved();

            // SmsAyarlari tablosundan aktif olan ilk kaydı al
            SmsAyarlari smsAyar = Session.FindObject<SmsAyarlari>(new BinaryOperator(nameof(SmsAyarlari.Aktif), true));
            if (smsAyar != null) { }
            string mesaj =  "Sayın "+MusteriAdi+" "+ fKayitTarihi+" tarihinde" + Gelinlik.ModelKarti.ModelAdi;
            if (smsAyar != null)
            {
                SmsGonderCS smsgndr = new SmsGonderCS();

                // SmsGonder metodu içinde gerekli parametreleri ayarla
                smsgndr.SmsGonder( smsAyar.KullaniciAdi, smsAyar.Sifre,mesaj,Telefonu, smsAyar.Baslik);
              
            }
        }

    }

}
