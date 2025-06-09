using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp; // Required for UserFriendlyException
using System.Linq; // Required for LINQ Sum

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class Faturalar
    {
        public Faturalar(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Initialize default values if necessary
            FaturaTarihi = DateTime.Today;
            // KdvOranı = 18; // Example default VAT rate
        }

        // TODO: Consider changing double types to decimal for monetary values for better precision.
        public void RecalculateTotals()
        {
            if (IsLoading || IsSaving) return;

            // Mal_Hizmet_ToplamTL is now set directly in OnChanged when SiparisKarti or InvoicedQuantity changes.
            // Or it can be calculated if Fiyat and InvoicedQuantity are set independently.
            // For example: Mal_Hizmet_ToplamTL = (double)SiparisKarti.Fiyat * InvoicedQuantity; (assuming SiparisKarti.Fiyat is decimal)

            // TODO: Determine the correct VAT rate. Using a fixed rate or a new field for it.
            // For now, let's assume KdvOrani property is used if set, otherwise a default (e.g., 18%).
            double kdvRate = KdvOranı > 0 ? KdvOranı / 100.0 : 0.18; // Default to 18% if not set

            HesaplananKdvTL = Mal_Hizmet_ToplamTL * kdvRate;
            VergiDahilTplmTL = Mal_Hizmet_ToplamTL + HesaplananKdvTL;
            OdenecekTutar = VergiDahilTplmTL; // This might be more complex with discounts, etc.

            // TODO: Implement similar logic for Dvz fields if currency conversion is needed.
        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            if (IsLoading) return;

            if (propertyName == nameof(SiparisKarti) && SiparisKarti != null)
            {
                // Automatic Data Transfer from SiparisKarti
                F_ParaBirimi = SiparisKarti.ParaBirimi;

                // Set Firma based on Toptan or Retail
                if (SiparisKarti.SiparisFoy != null)
                {
                    if (SiparisKarti.SiparisFoy.Toptan && SiparisKarti.SiparisFoy.ToptanMusteri != null)
                    {
                        this.Firma = SiparisKarti.SiparisFoy.ToptanMusteri;
                    }
                    else if (!SiparisKarti.SiparisFoy.Toptan && SiparisKarti.SiparisFoy.Musteriler != null)
                    {
                        // TODO: Faturalar.Firma is type Firmalar. SiparisKarti.SiparisFoy.Musteriler is likely a different customer type.
                        // This requires a way to link/convert/find a Firmalar record for the retail customer.
                        // For now, leaving Firma as null or add a specific placeholder if one exists.
                        // Or, add a new association on Faturalar for retail customers.
                        // this.Firma = null; // Or find appropriate Firmalar record.
                        // For the purpose of this task, we'll leave it unassigned with a comment.
                        // Consider logging this situation.
                    }
                }

                // Set InvoicedQuantity (initial setting or based on remaining)
                // This requires SiparisKarti.InvoicedQuantityTotal to be accurate.
                // For now, if it's a new invoice being associated, default to remaining or full SiparisAdet.
                if (Session.IsNewObject(this)) // Only set initial quantity if this is a new invoice
                {
                    int alreadyInvoiced = 0;
                    if (SiparisKarti.IsLoaded && SiparisKarti.Faturalars.IsLoaded) // Ensure collections are loaded
                    {
                        alreadyInvoiced = SiparisKarti.Faturalars.Where(f => f != this && !Session.IsNewObject(f)).Sum(f => f.InvoicedQuantity);
                    }
                    // If SiparisKarti.UpdateInvoicingStatus() runs before this, InvoicedQuantityTotal should be up-to-date.
                    // However, direct calculation here is safer for initialization.

                    int remainingQuantity = SiparisKarti.SiparisAdet - alreadyInvoiced;
                    InvoicedQuantity = remainingQuantity > 0 ? remainingQuantity : 0; // Default to remaining, ensure non-negative
                }


                // Calculate Mal_Hizmet_ToplamTL based on SiparisKarti.Fiyat and this.InvoicedQuantity
                // Ensure SiparisKarti.Fiyat is accessible and InvoicedQuantity is set.
                // TODO: Handle potential type mismatch (decimal vs double). Casting SiparisKarti.Fiyat to double.
                Mal_Hizmet_ToplamTL = (double)SiparisKarti.Fiyat * InvoicedQuantity;

                RecalculateTotals();
            }
            else if (propertyName == nameof(InvoicedQuantity) || propertyName == nameof(KdvOranı))
            {
                // If SiparisKarti is set, update Mal_Hizmet_ToplamTL based on its Fiyat
                if (SiparisKarti != null)
                {
                     Mal_Hizmet_ToplamTL = (double)SiparisKarti.Fiyat * InvoicedQuantity;
                }
                // Else, Mal_Hizmet_ToplamTL might be manually set or based on other items (not covered here)
                RecalculateTotals();
            }
        }

        protected override void OnSaving()
        {
            base.OnSaving();

            if (SiparisKarti != null && !SiparisKarti.Session.IsObjectToDelete(SiparisKarti))
            {
                // Over-invoicing Prevention
                // Sum InvoicedQuantity from other committed invoices for the same SiparisKarti
                int alreadyInvoicedQuantity = SiparisKarti.Faturalars
                    .Where(f => f != this && !Session.IsNewObject(f) && !Session.IsObjectToDelete(f)) // Exclude current new invoice and those marked for deletion
                    .Sum(f => f.InvoicedQuantity);

                if (this.InvoicedQuantity + alreadyInvoicedQuantity > SiparisKarti.SiparisAdet)
                {
                    int canStillInvoice = SiparisKarti.SiparisAdet - alreadyInvoicedQuantity;
                    canStillInvoice = canStillInvoice < 0 ? 0 : canStillInvoice; // ensure non-negative
                    throw new UserFriendlyException(
                        $"Over-invoicing is not allowed. " +
                        $"Quantity to invoice ({this.InvoicedQuantity}) plus already invoiced quantity ({alreadyInvoicedQuantity}) " +
                        $"exceeds order quantity ({SiparisKarti.SiparisAdet}). " +
                        $"You can invoice up to {canStillInvoice} more for this order item.");
                }
            }
        }

        protected override void OnSaved()
        {
            base.OnSaved();
            if (SiparisKarti != null && SiparisKarti.Session != null && !SiparisKarti.Session.IsObjectToDelete(SiparisKarti))
            {
                SiparisKarti.UpdateInvoicingStatus();
                // If SiparisKarti is from a different session or needs explicit save:
                if (SiparisKarti.Session.IsObjectMarkedForDeletion(SiparisKarti) == false && SiparisKarti.IsChanged)
                {
                     SiparisKarti.Session.Save(SiparisKarti);
                }
            }
        }

        protected override void OnDeleting()
        {
            SiparisKarti relatedSiparisKarti = this.SiparisKarti; // Store reference before it's potentially nullified

            base.OnDeleting(); // XPO might nullify associations here or during commit.

            // The actual update to SiparisKarti should happen after the session confirms the deletion.
            // This can be tricky. A common pattern is to handle it in a controller or Session_Committing event.
            // For now, we queue the update if the SiparisKarti is still accessible.
            // A more robust solution might involve a delayed execution or a specific controller action.
            if (relatedSiparisKarti != null && relatedSiparisKarti.Session != null && !relatedSiparisKarti.Session.IsObjectToDelete(relatedSiparisKarti))
            {
                // Mark SiparisKarti for an update. The actual sum will be correct once this Fatura is gone.
                // This relies on UpdateInvoicingStatus to correctly sum remaining invoices.
                relatedSiparisKarti.UpdateInvoicingStatus();
                if (relatedSiparisKarti.Session.IsObjectMarkedForDeletion(relatedSiparisKarti) == false && relatedSiparisKarti.IsChanged)
                {
                    // relatedSiparisKarti.Session.Save(relatedSiparisKarti); // This might be too early if delete isn't committed.
                    // It's better if SiparisKarti is saved in the same transaction commit that deletes the invoice.
                    // Often, XAF handles this if the object is dirtied.
                }
            }
        }
    }
}
