﻿using System;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using DevExpress.Persistent.BaseImpl;
using Task = System.Threading.Tasks.Task;
using DevExpress.Persistent.Base;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class Model_Maliyet
    {
        private decimal dlkur;
        private decimal gbpkur;
        private decimal eurokur;

        private DateTime lastFetchTime;
        private const int CacheDurationInMinutes = 60;

        public Model_Maliyet(Session session) : base(session) { }

        public override void AfterConstruction()
        {
            base.AfterConstruction();

            ParaBirimi = Session.FindObject<Parabirimi>(CriteriaOperator.Parse("P_Birimi = 'EUR'"));
            Tarih = DateTime.Today;

            if (!(Session is NestedUnitOfWork) && Session.DataLayer != null && Session.IsNewObject(this) && string.IsNullOrEmpty(StyleNo))
            {
                int deger = DistributedIdGeneratorHelper.Generate(Session.DataLayer, this.GetType().FullName, "MyServerPrefix");
                StyleNo = string.Format("{0:D6}", deger);
            }
        }

        protected override async void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);

            if (!Equals(oldValue, newValue)) // Gereksiz çalışmayı önlemek için kontrol
            {
                if (propertyName == "Tarih")
                {
                    await FetchExchangeRatesAsync();
                }

                if (propertyName == nameof(SabitEuroKuru))
                {
                    UpdateMaliyetKumas();
                }

                await UpdateMaliyetHesaplamalariAsync(); // Asenkron güncelleme
            }
        }

        private async Task FetchExchangeRatesAsync()
        {
            // Eğer kurlar önceden çekilmişse ve cache süresi dolmamışsa, verileri yeniden çekme
            if (DateTime.Now - lastFetchTime < TimeSpan.FromMinutes(CacheDurationInMinutes))
            {
                return; // Tamamlanmış bir Task döndürmeye gerek yok, async metodlarda sadece return yeterli
            }

            try
            {
                lastFetchTime = DateTime.Now;

                string url = Tarih.Date == DateTime.Today
                    ? "https://www.tcmb.gov.tr/kurlar/today.xml"
                    : $"https://www.tcmb.gov.tr/kurlar/{Tarih:yyyyMM}/{Tarih:ddMMyyyy}.xml";

                using var httpClient = new HttpClient();
                var xmlString = await httpClient.GetStringAsync(url);

                var xmldosya = new XmlDocument();
                xmldosya.LoadXml(xmlString);

                dlkur = decimal.Parse(xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteBuying")?.InnerXml ?? "0", CultureInfo.InvariantCulture);
                eurokur = decimal.Parse(xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteBuying")?.InnerXml ?? "0", CultureInfo.InvariantCulture);
                gbpkur = decimal.Parse(xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='GBP']/BanknoteBuying")?.InnerXml ?? "0", CultureInfo.InvariantCulture);

                // UI-bound properties (DolarKuru, EuroKuru, SterlinKuru) are updated directly in XAF/Blazor
                DolarKuru = dlkur;
                EuroKuru = eurokur;
                SterlinKuru = gbpkur;
            }
            catch (Exception)
            {
                // Hata yönetimi burada ele alınabilir
            }

            return; // async metotta sadece return kullanılır
        }

        private void UpdateMaliyetKumas()
        {
            foreach (var item in MaliyetKumasCollection)
            {
                if (item.ParaBirimi.P_Birimi == "EUR")
                {
                    var kullanilanKur = (decimal)SabitEuroKuru <= 0 ? EuroKuru : (decimal)SabitEuroKuru;
                    item.BirimFiyatTL = item.BirimFiyatDoviz * (double)kullanilanKur;
                    item.ToplamDoviz = item.BirimGramaj * item.BirimFiyatDoviz;
                    item.ToplamTL = item.BirimFiyatTL * item.BirimGramaj;
                }
            }
        }

        private async Task UpdateMaliyetHesaplamalariAsync()
        {
            await Task.Run(() =>
            {
                UpdateMaliyetHesaplamalari();
            });
        }

        private void UpdateMaliyetHesaplamalari()
        {
            if (ParaBirimi == null)
                return;

            double tplm = KumasTutarTL + iscilikTL + BaskiNakisTasTL + YikamaTL + NavlunTL + TestTL + MalzemeTL;
            GenelGiderTL = tplm + ((tplm * GenelGider) / 100);

            MaliyetTutariTL = GenelGiderTL + (GenelGiderTL * KesimFazlasi) / 100;
            FinansBedeliTL = (MaliyetTutariTL * FinansBedeli) / 100;
            KazancBdlTL = (MaliyetTutariTL * KazancBedeli) / 100;
            KomisyonBdlTL = (MaliyetTutariTL * Komisyon) / 100;
            ToplamTutarTL = MaliyetTutariTL + FinansBedeliTL + KazancBdlTL + KomisyonBdlTL;

            var kullanilanKur = GetKullanilanKur(ParaBirimi.P_Birimi);
            if (kullanilanKur > 0)
            {
                KumasTutarDoviz = KumasTutarTL / (double)kullanilanKur;
                iscilikDoviz = iscilikTL / (double)kullanilanKur;
                BaskiNakisTasDvz = BaskiNakisTasTL / (double)kullanilanKur;
                YikamaDvz = YikamaTL / (double)kullanilanKur;
                NavlunDvz = NavlunTL / (double)kullanilanKur;
                TestDvz = TestTL / (double)kullanilanKur;
                MalzemeDvz = MalzemeTL / (double)kullanilanKur;
                GenelGiderDvz = GenelGiderTL / (double)kullanilanKur;
                MaliyetTutariDvz = MaliyetTutariTL / (double)kullanilanKur;
                FinansBedeliDvz = FinansBedeliTL / (double)kullanilanKur;
                KazancBdlDvz = KazancBdlTL / (double)kullanilanKur;
                KomisyonBdlDvz = KomisyonBdlTL / (double)kullanilanKur;
                ToplamTutarDoviz = ToplamTutarTL / (double)kullanilanKur;
                TeklifEdilenTL = TeklifEdilenDoviz * (double)kullanilanKur;
            }
        }

        private decimal GetKullanilanKur(string paraBirimi)
        {
            return paraBirimi switch
            {
                "EUR" => SabitEuroKuru <= 0 ? EuroKuru : (decimal)SabitEuroKuru,
                "USD" => SabitDolarKur <= 0 ? DolarKuru : SabitDolarKur,
                "GBP" => SabitSterlinKuru <= 0 ? SterlinKuru : SabitSterlinKuru,
                _ => 0
            };
        }
    }
}
