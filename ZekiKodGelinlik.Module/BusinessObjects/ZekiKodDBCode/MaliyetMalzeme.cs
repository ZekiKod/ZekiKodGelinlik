using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class MaliyetMalzeme
    {
        public MaliyetMalzeme(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction();
            ParaBirimi = Session.FindObject<Parabirimi>(CriteriaOperator.Parse("P_Birimi = 'TL'"));
            Miktar = 1;

        }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            if (ModelMaliyet!=null)
            {

           
            try
            {
                

                if (ParaBirimi != null)
                {
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

                        ToplamDoviz = Miktar * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * Miktar;
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

                        ToplamDoviz = Miktar * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * Miktar;
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

                        ToplamDoviz = Miktar * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * Miktar;
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

                        ToplamDoviz = Miktar * BirimFiyatDoviz;
                        ToplamTL = BirimFiyatTL * Miktar;
                    }
                }

            }
            catch (Exception)
            {


            }

            try
            {
                double kmstplm;

                kmstplm = ModelMaliyet.MaliyetMalzemes.Sum(x => x.ToplamTL);
                ModelMaliyet.MalzemeTL = kmstplm;
              
                    
               



            }
            catch (Exception)
            {


                }
            }
            base.OnChanged(propertyName, oldValue, newValue);
        }
    }

}
