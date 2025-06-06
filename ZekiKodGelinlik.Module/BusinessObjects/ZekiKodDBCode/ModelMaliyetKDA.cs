﻿using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Linq;
using DevExpress.Persistent.BaseImpl;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    public partial class ModelMaliyetKDA
    {
        public ModelMaliyetKDA(Session session) : base(session)
        {
            
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

            ParaBirimi = Session.FindObject<Parabirimi>(CriteriaOperator.Parse("P_Birimi = 'TL'"));
        }
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            if (ModelMaliyet == null)
            {
                return;
            }

            try
            {
                // Veri yükleme durumu kontrol edilerek, operasyonlar güvenli hale getirildi
                if (!Session.IsObjectsLoading)
                {
                    CalculateBirimFiyat();
                    CalculateToplamlar();
                    CalculateMaliyetler();
                }
            }
            catch (Exception ex)
            {
                // Hataları yakala ve logla
                Console.WriteLine($"Error in OnChanged: {ex.Message}");
            }

            base.OnChanged(propertyName, oldValue, newValue);
        }

        private void CalculateBirimFiyat()
        {
            if (ParaBirimi == null) return;

            switch (ParaBirimi.P_Birimi)
            {
                case "EUR":
                    BirimFiyatTL = (Adet*BirimFiyatDoviz) * (ModelMaliyet.SabitEuroKuru > 0 ?
                                  (double)ModelMaliyet.SabitEuroKuru : (double)ModelMaliyet.EuroKuru);
                    break;
                case "USD":
                    BirimFiyatTL = (Adet * BirimFiyatDoviz) * (ModelMaliyet.SabitDolarKur > 0 ?
                                  (double)ModelMaliyet.SabitDolarKur : (double)ModelMaliyet.DolarKuru);
                    break;
                case "GBP":
                    BirimFiyatTL = (Adet * BirimFiyatDoviz) * (ModelMaliyet.SabitSterlinKuru > 0 ?
                                  (double)ModelMaliyet.SabitSterlinKuru : (double)ModelMaliyet.SterlinKuru);
                    break;
                case "TL":
                    BirimFiyatTL = (Adet * BirimFiyatDoviz);
                    break;
            }

            ToplamDoviz = BirimFiyatDoviz;
            ToplamTL = BirimFiyatTL;
        }

        private void CalculateToplamlar()
        {
            if (ModelMaliyet == null || ModelMaliyet.ModelMaliyetKDAs == null) return;

            //double yikamaToplam = ModelMaliyet.ModelMaliyetKDAs.Where(y => y.islem.islem == "YIKAMA").Sum(x => x.ToplamTL);
            double iscilikToplam = ModelMaliyet.ModelMaliyetKDAs.Sum(x => x.ToplamTL);
            //double navlunToplam = ModelMaliyet.ModelMaliyetKDAs.Where(y => y.islem.islem == "NAVLUN").Sum(x => x.ToplamTL);
            //double testToplam = ModelMaliyet.ModelMaliyetKDAs.Where(y => y.islem.islem == "TEST BEDELİ").Sum(x => x.ToplamTL);
            //double baskiNakisTasToplam = ModelMaliyet.ModelMaliyetKDAs.Where(y => y.islem.islem == "BASKI" || y.islem.islem == "NAKIŞ" || y.islem.islem == "TAŞLAMA").Sum(x => x.ToplamTL);

            //ModelMaliyet.YikamaTL = yikamaToplam;
            ModelMaliyet.iscilikTL = iscilikToplam;
            //ModelMaliyet.NavlunTL = navlunToplam;
            //ModelMaliyet.TestTL = testToplam;
            //ModelMaliyet.BaskiNakisTasTL = baskiNakisTasToplam;
        }

        private void CalculateMaliyetler()
        {
            if (ModelMaliyet == null || ParaBirimi == null) return;

            double kur = GetCurrentExchangeRate();

            ModelMaliyet.YikamaDvz = ModelMaliyet.YikamaTL / kur;
            ModelMaliyet.iscilikDoviz = ModelMaliyet.iscilikTL / kur;
            ModelMaliyet.NavlunDvz = ModelMaliyet.NavlunTL / kur;
            ModelMaliyet.TestDvz = ModelMaliyet.TestTL / kur;
            ModelMaliyet.BaskiNakisTasDvz = ModelMaliyet.BaskiNakisTasTL / kur;
        }

        private double GetCurrentExchangeRate()
        {
            double kur = 1;

            switch (ParaBirimi.P_Birimi)
            {
                case "EUR":
                    kur = ModelMaliyet.SabitEuroKuru > 0 ? (double)ModelMaliyet.SabitEuroKuru : (double)ModelMaliyet.EuroKuru;
                    break;
                case "USD":
                    kur = ModelMaliyet.SabitDolarKur > 0 ? (double)ModelMaliyet.SabitDolarKur : (double)ModelMaliyet.DolarKuru;
                    break;
                case "GBP":
                    kur = ModelMaliyet.SabitSterlinKuru > 0 ? (double)ModelMaliyet.SabitSterlinKuru : (double)ModelMaliyet.SterlinKuru;
                    break;
            }

            return kur;
        }
    }
}
