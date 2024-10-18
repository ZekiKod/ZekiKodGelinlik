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

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class SiparisKarti :XPObject
    {
        public SiparisKarti(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            //SiparisAdet = 1;
            //Fiyat = (decimal)ModelKarti.Model_Maliyets.Where(x => x.Onayli == true).FirstOrDefault().TeklifEdilenTL;
        }
        //protected override void TriggerObjectChanged(ObjectChangeEventArgs args)
        //{
        //    base.TriggerObjectChanged(args);

        //    if (args.PropertyName == nameof(OnaylanmısModel))
        //    {
        //        var sprkrt = new XPCollection<SiparisKarti>(Session);
        //        var sprs = sprkrt.Where(x => x.OnaylanmısModel == OnaylanmısModel).FirstOrDefault();

        //        if (sprs != null && sprs.OnaylanmısModel != null)
        //        {
        //            ParaBirimi = sprs.OnaylanmısModel.ParaBirimi;
        //            Fiyat = (decimal)sprs.OnaylanmısModel.TeklifEdilenDoviz;
        //        }
        //    }
        //}

        //protected override void OnChanged(string propertyName, object oldValue, object newValue)
        //{
        //    base.OnChanged(propertyName, oldValue, newValue);
        //    if (propertyName == nameof(ModelKarti))
        //    {
        //        if (ModelKarti!=null && ModelKarti.Model_Maliyets != null) { 
        //        OnaylanmısModel = ModelKarti.Model_Maliyets.Where(x => x.Onayli == true).FirstOrDefault();
        //            if (OnaylanmısModel != null)
        //            {
        //                Fiyat = (decimal)OnaylanmısModel.TeklifEdilenTL;
                       
        //            }
                
        //        }
        //    }
        //    if (propertyName == nameof(SiparisAdet) || propertyName == nameof(Fiyat) || propertyName == nameof(iskontoYuzde))
        //    {
        //        iskontoTutar = (SiparisAdet * Fiyat) * (iskontoYuzde / 100);
        //        ToplamTutar = (SiparisAdet * Fiyat) - iskontoTutar;
                
        //        if (SiparisFoy != null && SiparisFoy.SiparisKartis != null && !Session.IsObjectsLoading)
        //        {
        //            SiparisFoy.ToplamTutar = SiparisFoy.SiparisKartis.Sum(x => x.ToplamTutar);
        //        }
        //    }
        //}
        //protected override void TriggerObjectChanged(ObjectChangeEventArgs args)
        //{
        //    if (args.PropertyName == "OnaylanmısModel")
        //    {
        //        var sprkrt = new XPCollection<SiparisKarti>(Session);
        //        var sprs = sprkrt.Where(x => x.OnaylanmısModel == OnaylanmısModel).FirstOrDefault();


        //        if (sprs != null && sprs.OnaylanmısModel != null)
        //        {
        //            ParaBirimi = sprs.OnaylanmısModel.ParaBirimi;
        //            Fiyat = (decimal)sprs.OnaylanmısModel.TeklifEdilenDoviz;
        //        }

        //    }
        //}
        //protected override void OnChanged(string propertyName, object oldValue, object newValue)
        //{
        //    if (propertyName == nameof(SiparisAdet) || propertyName == nameof(Fiyat) || propertyName == nameof(iskontoYuzde))
        //    {
        //        iskontoTutar = (SiparisAdet * Fiyat) * (iskontoYuzde / 100);
        //        ToplamTutar = (SiparisAdet * Fiyat) - iskontoTutar;

        //        if (SiparisFoy != null && SiparisFoy.SiparisKartis != null && !Session.IsObjectsLoading)
        //        {
        //            // Yükleme işlemi sırasında yeniden yükleme yapılmaması için kontrol ekleyin
        //            SiparisFoy.ToplamTutar = SiparisFoy.SiparisKartis.Sum(x => x.ToplamTutar);
        //        }
        //    }

        //    base.OnChanged(propertyName, oldValue, newValue);
        //}
    }
}
