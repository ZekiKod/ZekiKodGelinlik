using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using System.Windows.Forms;
using System.Windows;
using System.Linq;
using DevExpress.Persistent.Validation;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class SiparisKarti : XPObject
    {
        public SiparisKarti(Session session) : base(session) { }

        [RuleRange(DefaultContexts.Save, 1, int.MaxValue, CustomMessageTemplate = "Sipariş adedi 0'dan büyük olmalıdır.")]
        public int SiparisAdet
        {
            get { return fSiparisAdet; }
            set { SetPropertyValue<int>(nameof(SiparisAdet), ref fSiparisAdet, value); }
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            //SiparisAdet = 1;
            //Fiyat = (decimal)ModelKarti.Model_Maliyets.Where(x => x.Onayli == true).FirstOrDefault().TeklifEdilenTL;
        }

        private void UpdateTotals()
        {
            iskontoTutar = (SiparisAdet * Fiyat) * (iskontoYuzde / 100);
            ToplamTutar = (SiparisAdet * Fiyat) - iskontoTutar;

            if (SiparisFoy != null && SiparisFoy.SiparisKartis != null && !Session.IsObjectsLoading)
            {
                SiparisFoy.ToplamTutar = SiparisFoy.SiparisKartis.Sum(x => x.ToplamTutar);
            }
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(ModelKarti))
            {
                if (ModelKarti != null && ModelKarti.Model_Maliyets != null)
                {
                    OnaylanmısModel = ModelKarti.Model_Maliyets.FirstOrDefault(x => x.Onayli == true);
                    if (OnaylanmısModel != null)
                    {
                        Fiyat = (decimal)OnaylanmısModel.TeklifEdilenTL;
                    }
                }
            }

            if (propertyName == nameof(SiparisAdet) || propertyName == nameof(Fiyat) || propertyName == nameof(iskontoYuzde))
            {
                UpdateTotals();
            }
        }
    }
}
