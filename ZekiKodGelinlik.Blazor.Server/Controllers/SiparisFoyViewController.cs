using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using Sirketiz.Module.BusinessObjects.Sirket_izDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;
using ZekiKodGelinlik.Module.BusinessObjects;

namespace ZekiKodGelinlik.Blazor.Server.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class SiparisFoyViewController : ObjectViewController<ListView, SiparisFoy>
    {
        // Use CodeRush to create Controllers and Actions with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/403133/
        public SiparisFoyViewController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            // Oturum açan kullanıcıyı al
            ApplicationUser currentUser = (ApplicationUser)SecuritySystem.CurrentUser;

            if (currentUser != null)
            {
                // Kullanıcının "Admin" rolüne sahip olup olmadığını kontrol et
                bool isAdmin = currentUser.Roles.Any(role => role.Name == "Admin");

                if (!isAdmin && currentUser.kisi_kartlari_to != null)
                {
                    // Eğer kullanıcı Admin değilse, yalnızca Temsilci olduğu verileri filtrele
                    kisi_kartlari kisiKartlariToInCurrentSession = ObjectSpace.GetObjectByKey<kisi_kartlari>(currentUser.kisi_kartlari_to.Oid);

                    // Yalnızca o kullanıcıya ait dataları filtrele
                    View.CollectionSource.Criteria["TemsilciFilter"] = CriteriaOperator.Parse("Temsilci = ?", kisiKartlariToInCurrentSession);
                }
                else
                {
                    // Kullanıcı admin ise tüm kayıtları görebilir, filtre uygulama
                    View.CollectionSource.Criteria.Clear();
                }
            }
        }
    }
}
