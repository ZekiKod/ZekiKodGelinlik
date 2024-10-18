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
    public partial class EvrakSablonListesi
    {
        public EvrakSablonListesi(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
