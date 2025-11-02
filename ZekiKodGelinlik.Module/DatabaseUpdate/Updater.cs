using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using ZekiKodGelinlik.Module.BusinessObjects;
using Microsoft.Extensions.DependencyInjection;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Module.DatabaseUpdate;

public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

#if !RELEASE
        var defaultRole = CreateDefaultRole();
        var adminRole = CreateAdminRole();
        var portalRole = CreatePortalUserRole(); // Portal rolünü oluştur

        ObjectSpace.CommitChanges();

        UserManager userManager = ObjectSpace.ServiceProvider.GetRequiredService<UserManager>();
        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, "User") == null) {
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "User", EmptyPassword, (user) => {
                user.Roles.Add(defaultRole);
            });
        }

        if(userManager.FindUserByName<ApplicationUser>(ObjectSpace, "Admin") == null) {
            string EmptyPassword = "";
            _ = userManager.CreateUser<ApplicationUser>(ObjectSpace, "Admin", EmptyPassword, (user) => {
                user.Roles.Add(adminRole);
            });
        }

        // Test için bir müşteri ve portal kullanıcısı oluştur
        if(userManager.FindUserByName<PortalUser>(ObjectSpace, "portaluser") == null)
        {
            Musteriler testMusteri = ObjectSpace.FirstOrDefault<Musteriler>(m => m.MusteriAdi == "Test Müşterisi");
            if(testMusteri == null)
            {
                testMusteri = ObjectSpace.CreateObject<Musteriler>();
                testMusteri.MusteriAdi = "Test Müşterisi";
                testMusteri.CariKodu = "TEST001";
            }

            string EmptyPassword = "";
             _ = userManager.CreateUser<PortalUser>(ObjectSpace, "portaluser", EmptyPassword, (user) => {
                user.Musteri = testMusteri;
                user.Roles.Add(portalRole);
            });
        }

        ObjectSpace.CommitChanges();
#endif
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
    }
    private PermissionPolicyRole CreateAdminRole() {
        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
            adminRole.IsAdministrative = true;
        }
        return adminRole;
    }

    private PermissionPolicyRole CreatePortalUserRole() {
        PortalUserRole portalRole = ObjectSpace.FirstOrDefault<PortalUserRole>(r => r.Name == "Portal Kullanıcısı");
        if(portalRole == null) {
            portalRole = ObjectSpace.CreateObject<PortalUserRole>();
            portalRole.Name = "Portal Kullanıcısı";

            // Sadece kendi müşteri kartını görme izni
            portalRole.AddObjectPermission<Musteriler>(SecurityOperations.Read, "[Oid] = CurrentUser.Musteri.Oid", SecurityPermissionState.Allow);

            // Sadece kendi siparişlerini görme, oluşturma ve düzenleme izni
            portalRole.AddObjectPermission<SiparisKarti>(SecurityOperations.CRUDAccess, "[Musteri.Oid] = CurrentUser.Musteri.Oid", SecurityPermissionState.Allow);

            // Sadece kendi faturalarını görme izni
            // Faturalar nesnesinde Müşteri ilişkisi olduğunu varsayıyoruz. Eğer yoksa bu kural çalışmaz.
            // portalRole.AddObjectPermission<Faturalar>(SecurityOperations.Read, "[Musteri.Oid] = CurrentUser.Musteri.Oid", SecurityPermissionState.Allow);

            // Sadece kendi destek taleplerini görme ve oluşturma izni
            // portalRole.AddObjectPermission<MusteriDestekTalebi>(SecurityOperations.CRUDAccess, "[Musteri.Oid] = CurrentUser.Musteri.Oid", SecurityPermissionState.Allow);

            // Diğer tüm verilere erişimi engelle
            portalRole.AddTypePermission<ApplicationUser>(SecurityOperations.Read, SecurityPermissionState.Deny);
            portalRole.AddTypePermission<PortalUser>(SecurityOperations.Read, SecurityPermissionState.Deny);
            portalRole.AddObjectPermission<PortalUser>(SecurityOperations.Read, "[Oid] = CurrentUserId()", SecurityPermissionState.Allow); // Kendi profilini görebilir
        }
        return portalRole;
    }

    private PermissionPolicyRole CreateDefaultRole() {
        PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

			defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
			defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
			defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermission<ModelDifference>(SecurityOperations.ReadWriteAccess, "UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
            defaultRole.AddObjectPermission<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, "Owner.UserId = ToStr(CurrentUserId())", SecurityPermissionState.Allow);
			defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserId == CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditedObjectWeakReference>(SecurityOperations.Read, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }
}
