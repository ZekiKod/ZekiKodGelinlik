﻿using System;
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
    public partial class puantaj_gun
    {
        public puantaj_gun(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }
    }

}
