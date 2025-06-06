﻿using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using ZekiKodGelinlik.Module.BusinessObjects;
using DevExpress.ExpressApp;
using Sirketiz.Module.BusinessObjects.Sirket_izDB;
using Microsoft.JSInterop;
using DevExpress.XtraSpellChecker.Native;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class SiparisFoy : XPObject
    {
        private IJSRuntime _jsRuntime;
        
        public SiparisFoy(Session session) : base(session)
        {

        }

        //public void SetJSRuntime(IJSRuntime jsRuntime)
        //{
        //    _jsRuntime = jsRuntime;
        //}
        //protected override async void AfterChangeByXPPropertyDescriptor()
        //{
        //    base.AfterChangeByXPPropertyDescriptor();

           

        //        // JavaScript işlemleri için _jsRuntime kullanın
        //        if (_jsRuntime != null)
        //        {
        //            try
        //            {
        //                await _jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
        //                await _jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
        //            }
        //            catch (Exception ex)
        //            {
        //                Tracing.Tracer.LogError($"JavaScript hatası: {ex.Message}");
        //            }
        //        }
            
        //}


        //public override void AfterConstruction()
        //{
        //    base.AfterConstruction();
        //    Tarih = DateTime.Now;
        //    OtoSiparisEkle = true;

        //    ApplicationUser currentUser = (ApplicationUser)SecuritySystem.CurrentUser;
        //    if (currentUser != null && currentUser.kisi_kartlari_to != null)
        //    {
        //        kisi_kartlari kisiKartlariToInCurrentSession = Session.GetObjectByKey<kisi_kartlari>(currentUser.kisi_kartlari_to.Oid);
        //        Temsilci = kisiKartlariToInCurrentSession;
        //    }

        //}

        //protected override async void OnChanged(string propertyName, object oldValue, object newValue)
        //{
        //    if (!Session.IsObjectsLoading)
        //    {
        //        if (OtoSiparisEkle && !string.IsNullOrEmpty(ModelBarkod) && propertyName == "ModelBarkod")
        //        {
        //            // ModelBarkod'a karşılık gelen ModelNo'yu bul
        //            ModelKarti modelKarti = Session.FindObject<ModelKarti>(CriteriaOperator.Parse("ModelNo == ?", ModelBarkod));

        //            if (modelKarti != null)
        //            {
        //                // Yeni sipariş ekle
        //                SiparisKarti yeniSiparis = new SiparisKarti(Session)
        //                {
        //                    ModelKarti = modelKarti
        //                };

        //                SiparisKartis.Add(yeniSiparis);



        //                ModelBarkod = "";
                        
        //                // ToplamTutar'ı güncelle
        //                ToplamTutar = SiparisKartis.Sum(x => x.ToplamTutar);
        //                //await _jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
        //                //await _jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
        //            }
        //        }

        //        // Diğer alanların hesaplamalarını güncelle
        //        if (propertyName == nameof(ToplamTutar) || propertyName == nameof(iskontoYuzde) || propertyName == nameof(KdvOranYuzde))
        //        {
        //            KdvTutar = 0;
        //            if (SiparisKartis != null && !Session.IsObjectsLoading)
        //            {
        //                iskontoTutar = SiparisKartis.Where(x => x.iskontoTutar == 0).Sum(x => x.ToplamTutar) * (iskontoYuzde / 100);
        //                if (KdvOranYuzde != null)
        //                {
        //                    KdvTutar = ((ToplamTutar - iskontoTutar) / 100) * KdvOranYuzde.Kdv;
        //                }

        //                GenelToplam = (ToplamTutar - iskontoTutar) + KdvTutar;
        //                KalanOdeme = GenelToplam - FoyOdemePlanis.Sum(x => x.Tutar);
        //            }
        //        }
              
        //        // Değişiklikleri kaydet
        //        Session.Save(this);
             
        //        //// JavaScript kullanarak ModelBarkod alanını temizle ve odakla
        //        //if (_jsRuntime != null)
        //        //{
        //        //    try
        //        //    {
        //        //        await _jsRuntime.InvokeVoidAsync("JSFunctions.clearElementById", "ModelBarkod");
        //        //        await _jsRuntime.InvokeVoidAsync("JSFunctions.focusElementById", "ModelBarkod");
        //        //    }
        //        //    catch (Exception ex)
        //        //    {
        //        //        Tracing.Tracer.LogError($"JavaScript hatası: {ex.Message}");
        //        //    }
        //        //}
        //    }
        //}
    }
}
