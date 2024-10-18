using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class KumasCinsi
    {
        public KumasCinsi(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
