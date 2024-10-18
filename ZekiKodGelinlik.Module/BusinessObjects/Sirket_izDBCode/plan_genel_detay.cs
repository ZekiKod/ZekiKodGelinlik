using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace Sirketiz.Module.BusinessObjects.Sirket_izDB
{

    public partial class plan_genel_detay
    {
        public plan_genel_detay(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
