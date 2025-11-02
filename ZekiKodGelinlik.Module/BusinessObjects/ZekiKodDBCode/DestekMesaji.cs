using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.ExpressApp;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class DestekMesaji
    {
        public DestekMesaji(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            GonderimTarihi = DateTime.Now;

            // Mesajı gönderen kişinin kullanıcı adını otomatik olarak ata
            if (SecuritySystem.CurrentUser != null)
            {
                Gonderen = SecuritySystem.CurrentUserName;
            }
        }
    }

}
