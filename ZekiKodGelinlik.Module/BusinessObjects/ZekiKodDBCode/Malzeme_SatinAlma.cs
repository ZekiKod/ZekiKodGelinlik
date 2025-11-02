using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class Malzeme_SatinAlma
    {
        public Malzeme_SatinAlma(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(GelenMiktar) && newValue != null && (double)newValue > 0)
            {
                UpdateMalzemeStok((double)newValue - (double)oldValue);
            }
        }

        private void UpdateMalzemeStok(double changeAmount)
        {
            if (Malzeme == null || Depo == null)
            {
                return;
            }

            MalzemeStok stok = Session.FindObject<MalzemeStok>(
                CriteriaOperator.Parse("Malzeme = ? AND Depo = ?", Malzeme.Oid, Depo.Oid));

            if (stok == null)
            {
                stok = new MalzemeStok(Session);
                stok.Malzeme = Malzeme;
                stok.Depo = Depo;
            }

            stok.Stok += changeAmount;
        }
    }

}
