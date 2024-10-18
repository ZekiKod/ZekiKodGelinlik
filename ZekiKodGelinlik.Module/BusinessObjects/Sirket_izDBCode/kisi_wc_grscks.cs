using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;

namespace Sirketiz.Module.BusinessObjects.Sirket_izDB
{
    [DefaultClassOptions]
    public partial class kisi_wc_grscks
    {
        public kisi_wc_grscks(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
