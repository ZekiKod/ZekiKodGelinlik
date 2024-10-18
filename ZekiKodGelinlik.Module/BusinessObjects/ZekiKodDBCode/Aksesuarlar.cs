using System;
using System.Linq;
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
    public partial class Aksesuarlar
    {
      

        public Aksesuarlar(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }


      



    //}

    //protected override void OnSaving()
    //{
    //    base.OnSaving();
    //    string ktgori = Kategori.Kategori;
    //    string rnk = Renk.RenkAdi;

    //    try
    //    {
    //        Aksesuar = ktgori + " " + rnk + " En:" + En + " Boy:" + Boy + " Kalınlık:" + Kalinlik;
    //    }
    //    catch (Exception)
    //    {


    //    }

    //}
}

}
