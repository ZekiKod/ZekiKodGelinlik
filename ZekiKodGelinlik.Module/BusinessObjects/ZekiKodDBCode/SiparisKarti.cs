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
using System.Linq; // Added for LINQ Sum

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
            UpdateSprDurumu(); // Initialize production status on creation
            InvoicedQuantityTotal = 0;
            InvoicingStatus = "Not Invoiced"; // Initial invoicing status
            UpdateInvoicingStatus(); // Initialize invoicing status properly based on any existing data (though unlikely for new)
        }

        public void UpdateKesimlenToplam()
        {
            if (IsLoading || IsSaving) return; // Avoid updates during loading/saving
            int total = 0;
            foreach (GunlukKesim kesim in GunlukKesims)
            {
                // Assuming KesilenAdet is the correct field.
                // If not, sum all size fields: kesim.XXS + kesim.XS + ...
                total += kesim.KesilenAdet;
            }
            KesimlenToplam = total;
        }

        public void UpdateDikimToplam()
        {
            if (IsLoading || IsSaving) return; // Avoid updates during loading/saving
            int total = 0;
            foreach (Dikim dikim in Dikims)
            {
                total += dikim.CikanAdet;
            }
            DikimToplam = total;
        }

        private void UpdateSprDurumu()
        {
            if (IsLoading && !Session.IsNewObject(this)) // Allow update during loading for new objects to set initial state
            {
                return;
            }

            string desiredStatusString = "";
            if (PaketToplam >= SiparisAdet)
            {
                desiredStatusString = "Tamamlandı";
            }
            else if (DikimToplam >= SiparisAdet)
            {
                desiredStatusString = "Paketlemede";
            }
            else if (KesimlenToplam >= SiparisAdet)
            {
                desiredStatusString = "Dikimde";
            }
            else if (KesimlenToplam > 0)
            {
                desiredStatusString = "Kesimde";
            }
            else
            {
                desiredStatusString = "Kesim Bekliyor";
            }

            if (!string.IsNullOrEmpty(desiredStatusString))
            {
                // Assumption: SiparisDurumu object has a string property named 'Durumu' that uniquely identifies the status.
                // If the property is named differently (e.g., 'Name', 'StatusText'), adjust the CriteriaOperator accordingly.
                SiparisDurumu statusObject = Session.FindObject<SiparisDurumu>(CriteriaOperator.Parse("Durumu == ?", desiredStatusString));

                if (statusObject != null)
                {
                    this.SprDurumu = statusObject;
                }
                else
                {
                    // TODO: Log this event if possible.
                    // This case means a SiparisKarti's state maps to a SiparisDurumu string (e.g., "Kesimde")
                    // but no corresponding SiparisDurumu object exists in the database.
                    // Action: The SiparisDurumu table needs to be seeded with an entry for 'desiredStatusString'.
                    // For now, SprDurumu will remain unchanged or be null if not previously set.
                    // Consider setting to a default status or throwing an error based on business rules if a status MUST be set.
                    // For safety, if SprDurumu was previously set, we leave it, otherwise it might become null.
                    // this.SprDurumu = null; // Or a default status if one exists and is appropriate.
                }
            }
            // If desiredStatusString is empty (should not happen with current logic), SprDurumu remains unchanged.
        }

        public void UpdateInvoicingStatus()
        {
            if (IsLoading && !Session.IsNewObject(this)) return;

            int currentInvoicedTotal = 0;
            if (Faturalars.IsLoaded) // Ensure collection is loaded before iterating
            {
                 // Sum InvoicedQuantity from all Faturalar related to this SiparisKarti
                // Consider only committed invoices if a status field exists on Faturalar (e.g., IsCommitted, Status)
                currentInvoicedTotal = Faturalars.Where(f => !Session.IsObjectToDelete(f)).Sum(f => f.InvoicedQuantity);
            }
            else
            {
                // If collection is not loaded, this might be problematic.
                // Forcing load, but be careful with performance implications in other scenarios.
                // This is important for correctness when called from OnLoaded or OnSaving.
                Faturalars.Load();
                currentInvoicedTotal = Faturalars.Where(f => !Session.IsObjectToDelete(f)).Sum(f => f.InvoicedQuantity);

            }
            InvoicedQuantityTotal = currentInvoicedTotal;

            if (InvoicedQuantityTotal == 0)
            {
                InvoicingStatus = "Not Invoiced";
            }
            else if (InvoicedQuantityTotal > 0 && InvoicedQuantityTotal < SiparisAdet)
            {
                InvoicingStatus = "Partially Invoiced";
            }
            else if (InvoicedQuantityTotal >= SiparisAdet)
            {
                // This also covers InvoicedQuantityTotal == SiparisAdet
                InvoicingStatus = "Fully Invoiced";
            }

            // Safeguard check, though Faturalar.OnSaving should prevent this state.
            if (InvoicedQuantityTotal > SiparisAdet)
            {
                InvoicingStatus = "Error: Over-Invoiced";
            }
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            if (!IsLoading && !IsSaving)
            {
                if (propertyName == nameof(SiparisAdet) ||
                    propertyName == nameof(KesimlenToplam) ||
                    propertyName == nameof(DikimToplam) ||
                    propertyName == nameof(PaketToplam))
                {
                    UpdateSprDurumu();
                }

                // Financial calculations (existing logic, ensure it's still relevant)
                if (propertyName == nameof(SiparisAdet) || propertyName == nameof(Fiyat) || propertyName == nameof(iskontoYuzde))
                {
                    iskontoTutar = (SiparisAdet * Fiyat) * (iskontoYuzde / 100m); // Use 100m for decimal division
                    ToplamTutar = (SiparisAdet * Fiyat) - iskontoTutar;

                    if (SiparisFoy != null && !Session.IsObjectsLoading)
                    {
                        SiparisFoy.UpdateFinancialTotals();
                    }
                }
            }
        }

        // It's generally better to handle collection changes by responding to their ListChanged event
        // or by overriding methods like OnCollectionChanged if your XPO version supports it.
        // For simplicity in this step, we might need to call UpdateKesimlenToplam and UpdateDikimToplam
        // after operations that modify these collections elsewhere, or periodically.
        // A more direct way within SiparisKarti would be if GunlukKesim/Dikim objects notify SiparisKarti.
        // For now, let's assume these will be called from elsewhere or we add a manual refresh mechanism if needed.
        // However, if GunlukKesim or Dikim objects are modified *through* the SiparisKarti's collections,
        // XPO might trigger OnChanged for the collection property itself, which we can handle.

        // Placeholder for SprDurumu as string, as SiparisDurumu enum is not defined yet
        // This property will not be persistent if SprDurumu (the object type) is the one mapped to DB.
        // This is a temporary workaround. The actual SprDurumu (object) should be updated.
        // string fSprDurumuString; // Removed as per new requirement
        // [NonPersistent] // So it doesn't conflict with the actual 'SprDurumu' object property
        // public string SprDurumuString // Removed as per new requirement
        // {
        //     get { return fSprDurumuString; }
        //     // set { SetPropertyValue<string>(nameof(SprDurumuString), ref fSprDurumuString, value); } // Setter handled by UpdateSprDurumu
        // }
         protected override void OnSaving()
        {
            UpdateKesimlenToplam();
            UpdateDikimToplam();
            // PaketToplam update logic would go here if it were defined
            UpdateSprDurumu(); // Ensure production status is correct before saving
            UpdateInvoicingStatus(); // Ensure invoicing status is correct before saving
            base.OnSaving();
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            UpdateInvoicingStatus(); // Initialize/Refresh invoicing status when object is loaded
            UpdateSprDurumu();      // Initialize/Refresh production status when object is loaded
        }
    }
}
