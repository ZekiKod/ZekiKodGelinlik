using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using System.Linq;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKod.Module.Controllers
{
	public class SiparisFoyViewController : ViewController
	{
		public SiparisFoyViewController()
		{
			TargetObjectType = typeof(SiparisFoy);
		}

		protected override void OnActivated()
		{
			base.OnActivated();
			View.ControlsCreated += View_ControlsCreated;
		}

		private void View_ControlsCreated(object sender, System.EventArgs e)
		{
			// View.Id'yi burada kontrol edebilirsiniz
			if (View.Id == "SiparisFoy_ListView_Prakende")
			{
				// SiparisFoy_ListView için iş mantığı: Toptan alanını true yap
				foreach (var siparisFoy in View.SelectedObjects.OfType<SiparisFoy>())
				{
					siparisFoy.Toptan = false;
				}
			}
			else if (View.Id == "SiparisFoy_ListView"|| View.Id == "SiparisFoy_DetailView")

            {
				// SiparisFoy_ListView için iş mantığı: Toptan alanını true yap
				foreach (var siparisFoy in View.SelectedObjects.OfType<SiparisFoy>())
				{
					siparisFoy.Toptan = true;
				}

			}
		}

		private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
		{
			if (e.Object is SiparisFoy siparisFoy)
			{
				if (e.PropertyName == nameof(SiparisFoy.ToplamTutar) || e.PropertyName == nameof(SiparisFoy.iskontoYuzde))
				{
					// Genel iş mantığı
					if (siparisFoy.SiparisKartis != null && !siparisFoy.Session.IsObjectsLoading)
					{
						siparisFoy.iskontoTutar = siparisFoy.SiparisKartis.Where(x => x.iskontoTutar == 0).Sum(x => x.ToplamTutar) * (siparisFoy.iskontoYuzde / 100);
						siparisFoy.GenelToplam = siparisFoy.ToplamTutar - siparisFoy.iskontoTutar;
					}
				}
			}
		}

		protected override void OnDeactivated()
		{
			View.ControlsCreated -= View_ControlsCreated;
			ObjectSpace.ObjectChanged -= ObjectSpace_ObjectChanged;
			base.OnDeactivated();
		}
	}
}
