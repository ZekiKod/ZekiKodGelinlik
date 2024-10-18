using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using System.IO;

using System.Configuration;
using System.Linq;

namespace Sirketiz.Module.BusinessObjects.Sirket_izDB
{
    [DefaultClassOptions]
    public partial class kisi_grscks
    {
        public kisi_grscks(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
       

      
    }

}
