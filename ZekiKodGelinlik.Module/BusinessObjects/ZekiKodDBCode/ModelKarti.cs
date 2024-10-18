using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using System.Linq;
using DevExpress.ExpressApp;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class ModelKarti
    {
        //private string mdlgrubu;

        public ModelKarti(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction();
            if (SecuritySystem.CurrentUserId!=null)
            {
                //Tasarimci = (User)Session.GetObjectByKey(SecuritySystem.UserType, SecuritySystem.CurrentUserId);
            }
            
        }
        //protected override void OnSaving()
        //{
        //    if (!(Session is NestedUnitOfWork) && Session.DataLayer != null && Session.IsNewObject(this) && string.IsNullOrEmpty(ModelNo))
        //    {
        //        int deger = DistributedIdGeneratorHelper.Generate(Session.DataLayer, this.GetType().FullName, "MyServerPrefix");
        //        ModelNo = string.Format("{0:D6}", deger);
        //    }
        //    base.OnSaving();
        //}

    }

}
