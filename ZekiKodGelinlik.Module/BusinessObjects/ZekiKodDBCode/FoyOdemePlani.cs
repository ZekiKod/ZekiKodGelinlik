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
            if (propertyName == nameof(Tutar))
            {
                if (SiparisFoy != null) { 
                SiparisFoy.KalanOdeme = SiparisFoy.GenelToplam - SiparisFoy.FoyOdemePlanis.Sum(x => x.Tutar);
                }
                base.OnChanged();
            }
        }


    }

}
