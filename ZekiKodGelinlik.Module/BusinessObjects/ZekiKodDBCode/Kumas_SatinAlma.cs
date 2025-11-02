using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class Kumas_SatinAlma
    {
        public Kumas_SatinAlma(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(GelenKumas) && newValue != null && (double)newValue > 0)
            {
                UpdateKumasStok((double)newValue - (double)oldValue);
            }
        }

        private void UpdateKumasStok(double changeAmount)
        {
            if (Kumas_Karti == null || Depo == null)
            {
                return;
            }

            KumasStok stok = Session.FindObject<KumasStok>(
                CriteriaOperator.Parse("Kumas = ? AND Depo = ?", Kumas_Karti.Oid, Depo.DepoAdi));

            if (stok == null)
            {
                stok = new KumasStok(Session);
                stok.Kumas = Kumas_Karti;
                stok.Depo = Depo.DepoAdi;
            }

            stok.Stok += changeAmount;
        }
    }

}
