using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class ProvaRandevu
    {
        public ProvaRandevu(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Yeni randevu oluşturulduğunda başlangıç ve bitiş zamanlarını ayarla
            StartTime = DateTime.Now;
            EndTime = DateTime.Now.AddHours(1);
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            // Kaydederken, eğer konu boşsa veya siparişle ilgili değilse, konuyu otomatik olarak ayarla
            if (SiparisKarti != null && (string.IsNullOrEmpty(Subject) || !Subject.Contains(SiparisKarti.MusteriAdi)))
            {
                Subject = $"{SiparisKarti.MusteriAdi} - Prova Randevusu";
            }
        }
    }

}
