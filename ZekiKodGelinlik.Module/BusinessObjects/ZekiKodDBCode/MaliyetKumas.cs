using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using DevExpress.Persistent.Base;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class MaliyetKumas
    {
        public MaliyetKumas(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction();
            //ParaBirimi = Session.FindObject<Parabirimi>(CriteriaOperator.Parse("P_Birimi = 'EUR'"));
        }


        protected override void TriggerObjectChanged(ObjectChangeEventArgs args)
        {
            base.TriggerObjectChanged(args);

            if (args.PropertyName == nameof(Kumas))
            {
                // KumasKarti nesnesini al
                var mkms = new XPCollection<KumasKarti>(Session);
                var kms = mkms.FirstOrDefault(x => x.Oid == Kumas.Oid);

                if (kms != null)
                {
                    // En büyük tarihi bul
                    var enBüyükTarih = kms.KumasFiyatlaris
                        .OrderByDescending(x => x.Tarih)
                        .FirstOrDefault();

                    // En büyük tarihi kullan
                    if (enBüyükTarih != null)
                    {
                        ParaBirimi = enBüyükTarih.ParaBirimi;
                        fBirimFiyatDoviz = (double)enBüyükTarih.Fiyati;
                    }
                }
            }
        }

        //protected override void OnChanged(string propertyName, object oldValue, object newValue)
        //{
        //    base.OnChanged(propertyName, oldValue, newValue);
        //    if (propertyName == nameof(ModelKarti))
        //    {
        //        if (ModelKarti != null && ModelKarti.Model_Maliyets != null)
        //        {
        //            OnaylanmısModel = ModelKarti.Model_Maliyets.Where(x => x.Onayli == true).FirstOrDefault();
        //            if (OnaylanmısModel != null)
        //            {
        //                Fiyat = (decimal)OnaylanmısModel.TeklifEdilenTL;
        //            }

        //        }
        //    }
        //    if (propertyName == nameof(SiparisAdet) || propertyName == nameof(Fiyat) || propertyName == nameof(iskontoYuzde))
        //    {
        //        iskontoTutar = (SiparisAdet * Fiyat) * (iskontoYuzde / 100);
        //        ToplamTutar = (SiparisAdet * Fiyat) - iskontoTutar;

        //        if (SiparisFoy != null && SiparisFoy.SiparisKartis != null && !Session.IsObjectsLoading)
        //        {
        //            SiparisFoy.ToplamTutar = SiparisFoy.SiparisKartis.Sum(x => x.ToplamTutar);
        //        }
        //    }
        //}

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            try
            {



                if (ParaBirimi != null)
                {
                    if (ModelMaliyet != null) { 
                        if (ParaBirimi.P_Birimi == "EUR")
                    {
                        if (ModelMaliyet.SabitEuroKuru <= 0)
                        {
                            BirimFiyatTL = BirimFiyatDoviz * (double)ModelMaliyet.EuroKuru;
                        }
                        else
                        {
                            BirimFiyatTL = BirimFiyatDoviz * (double)ModelMaliyet.SabitEuroKuru;
                        }

                        ToplamDoviz = BirimGramaj * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * BirimGramaj;
                    }
                    if (ParaBirimi.P_Birimi == "USD")
                    {
                        if (ModelMaliyet.SabitDolarKur <= 0)
                        {
                            BirimFiyatTL = BirimFiyatDoviz * (double)ModelMaliyet.DolarKuru;
                        }
                        else
                        {
                            BirimFiyatTL = BirimFiyatDoviz * (double)ModelMaliyet.SabitDolarKur;
                        }

                        ToplamDoviz = BirimGramaj * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * BirimGramaj;
                    }
                    if (ParaBirimi.P_Birimi == "GBP")
                    {
                        if (ModelMaliyet.SabitSterlinKuru <= 0)
                        {
                            BirimFiyatTL = BirimFiyatDoviz * (double)ModelMaliyet.SterlinKuru;
                        }
                        else
                        {
                            BirimFiyatTL = BirimFiyatDoviz * (double)ModelMaliyet.SabitSterlinKuru;
                        }

                        ToplamDoviz = BirimGramaj * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * BirimGramaj;
                    }
                    if (ParaBirimi.P_Birimi == "TL")
                    {
                        if (ModelMaliyet.SabitSterlinKuru <= 0)
                        {
                            BirimFiyatTL = BirimFiyatDoviz;
                        }
                        else
                        {
                            BirimFiyatTL = BirimFiyatDoviz;
                        }

                        ToplamDoviz = BirimGramaj * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * BirimGramaj;
                    }
                    }
                }


            }
            catch (Exception)
            {


            }
            try
            {
                double kmstplm;
                if (ModelMaliyet != null)
                {


                    kmstplm = ModelMaliyet.MaliyetKumasCollection.Sum(x => x.ToplamTL);
                    ModelMaliyet.KumasTutarTL = kmstplm;



                }


            }
            catch (Exception)
            {


            }

            base.OnChanged(propertyName, oldValue, newValue);

        }
    }

}
