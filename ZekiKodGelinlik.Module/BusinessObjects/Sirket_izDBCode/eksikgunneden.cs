using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace Sirketiz.Module.BusinessObjects.Sirket_izDB
{

    public partial class eksikgunneden
    {
        public eksikgunneden(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
