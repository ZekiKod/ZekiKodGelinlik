using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class FoyOdemePlani
    {
       
        public FoyOdemePlani(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (!Session.IsObjectsLoading) // Good practice to include this check
            {
                if (propertyName == nameof(Tutar))
                {
                    if (SiparisFoy != null)
                    {
                        SiparisFoy.UpdateFinancialTotals();
                    }
                }
            }
        }

        protected override void OnDeleting()
        {
            // Update SiparisFoy totals before this payment plan is removed from its collection by the deletion process.
            if (SiparisFoy != null && !SiparisFoy.Session.IsObjectToDelete(SiparisFoy) && !this.Session.IsObjectToDelete(this))
            {
                SiparisFoy.UpdateFinancialTotals();
            }
            base.OnDeleting();
        }
    }

}
