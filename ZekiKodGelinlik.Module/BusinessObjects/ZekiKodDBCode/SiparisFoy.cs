using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using ZekiKodGelinlik.Module.BusinessObjects; // For ApplicationUser if used
using DevExpress.ExpressApp;
using Sirketiz.Module.BusinessObjects.Sirket_izDB; // For kisi_kartlari if used
using System.Linq; // For .Sum()

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class SiparisFoy : XPObject
    {
        // private Microsoft.JSInterop.IJSRuntime _jsRuntime; // Keep if still used, remove if not

        public SiparisFoy(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Tarih = DateTime.Now;
            OtoSiparisEkle = true; // Assuming this is a desired default

            // Example of setting Temsilci based on current user - re-enable if ApplicationUser and SecuritySystem are configured
            // ApplicationUser currentUser = Session.GetObjectByKey<ApplicationUser>(SecuritySystem.CurrentUserId);
            // if (currentUser != null && currentUser.kisi_kartlari_to != null)
            // {
            //     Temsilci = Session.GetObjectByKey<kisi_kartlari>(currentUser.kisi_kartlari_to.Oid);
            // }
            UpdateFinancialTotals(); // Initialize totals
        }

        public void UpdateFinancialTotals()
        {
            // Prevent calculations during object loading unless it's a new object where defaults might apply via collections.
            if (Session.IsObjectsLoading && !Session.IsNewObject(this))
                return;

            // Ensure collections are loaded before summing. This is crucial.
            if (!SiparisKartis.IsLoaded) SiparisKartis.Load();
            if (!FoyOdemePlanis.IsLoaded) FoyOdemePlanis.Load();

            // Calculate ToplamTutar from SiparisKarti items
            ToplamTutar = SiparisKartis.Where(sk => sk != null && !sk.Session.IsObjectToDelete(sk)).Sum(sk => sk.ToplamTutar);
            
            decimal currentIskontoYuzde = iskontoYuzde < 0 ? 0 : iskontoYuzde; // Ensure non-negative
            iskontoTutar = ToplamTutar * (currentIskontoYuzde / 100m);

            if (KdvOranYuzde != null)
            {
                // Assuming KdvOranYuzde.Kdv holds the percentage value (e.g., 18 for 18%)
                KdvTutar = ((ToplamTutar - iskontoTutar) / 100m) * KdvOranYuzde.Kdv;
            }
            else
            {
                KdvTutar = 0;
            }

            GenelToplam = (ToplamTutar - iskontoTutar) + KdvTutar;

            KalanOdeme = GenelToplam - FoyOdemePlanis.Where(fop => fop != null && !fop.Session.IsObjectToDelete(fop)).Sum(fop => fop.Tutar);
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (Session.IsObjectsLoading) return;

            // Logic for adding SiparisKarti via ModelBarkod
            if (propertyName == nameof(ModelBarkod) && OtoSiparisEkle && !string.IsNullOrEmpty(ModelBarkod))
            {
                ModelKarti modelKarti = Session.FindObject<ModelKarti>(CriteriaOperator.Parse("ModelNo == ?", ModelBarkod));
                if (modelKarti != null)
                {
                    SiparisKarti yeniSiparis = new SiparisKarti(Session)
                    {
                        ModelKarti = modelKarti
                        // Initialize other SiparisKarti properties as needed
                    };
                    SiparisKartis.Add(yeniSiparis);
                    // After adding a new SiparisKarti, its own changes (like setting its ToplamTutar)
                    // should trigger SiparisFoy.UpdateFinancialTotals via SiparisKarti's OnChanged method.
                    // If not, uncomment the line below.
                    // UpdateFinancialTotals();
                    ModelBarkod = ""; // Clear ModelBarkod after processing
                }
            }

            // Properties that directly affect financial totals of SiparisFoy itself
            if (propertyName == nameof(iskontoYuzde) || propertyName == nameof(KdvOranYuzde))
            {
                UpdateFinancialTotals();
            }
            // ToplamTutar is an aggregate of SiparisKarti.ToplamTutar.
            // KalanOdeme is an aggregate involving FoyOdemePlani.Tutar.
            // These are updated when their respective child objects change and call UpdateFinancialTotals on the parent SiparisFoy.
        }

        protected override void OnSaving()
        {
            UpdateFinancialTotals(); // Ensure all totals are correct before saving
            base.OnSaving();
        }

        // IJSRuntime related code, keep if used, remove if not.
        // public void SetJSRuntime(Microsoft.JSInterop.IJSRuntime jsRuntime)
        // {
        //     _jsRuntime = jsRuntime;
        // }
        // protected override async void AfterChangeByXPPropertyDescriptor()
        // {
        //    base.AfterChangeByXPPropertyDescriptor();
        //    if (_jsRuntime != null) { /* ... JSRuntime logic ... */ }
        // }
    }
}
