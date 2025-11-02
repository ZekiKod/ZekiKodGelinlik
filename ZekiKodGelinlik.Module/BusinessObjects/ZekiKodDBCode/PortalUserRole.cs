using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class PortalUserRole
    {
        public PortalUserRole(Session session) : base(session) { }
        public override void AfterConstruction() {
            base.AfterConstruction();
            this.Name = "Portal Kullanıcısı";
            this.IsAdministrative = false;
        }
    }

}
