using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;
using System.Xml;
using System.Globalization;
using System.Linq;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.BaseImpl;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class AksesuarSiparis
    {
        public AksesuarSiparis(Session session) : base(session) { 
        
        }
        public override void AfterConstruction() {
            try
            {

                M_ParaBirimi = Session.FindObject<Parabirimi>(CriteriaOperator.Parse("P_Birimi = 'EUR'"));
                //M_KurTarih = DateTime.Today;
                FireOran = 5;
            }
            catch (Exception)
            {

                
            }
            base.AfterConstruction(); 
        }

        [Appearance("RuleMethod", AppearanceItemType = "ViewItem", TargetItems = "*", Context = "ListView",
   BackColor = "Yellow", FontColor = "Black")]

        public bool RuleMethod()
        {
            if (SiparisVerildi == true && GelenAksesuar <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [Appearance("RuleMethod2", AppearanceItemType = "ViewItem", TargetItems = "*", Context = "ListView",
   BackColor = "ORANGE", FontColor = "Black")]
        public bool RuleMethod2()
        {
            if (GelenAksesuar < SiparisEdilen && GelenAksesuar > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        [Appearance("RuleMethod3", AppearanceItemType = "ViewItem", TargetItems = "*", Context = "ListView",
   BackColor = "GREEN", FontColor = "Black")]
        public bool RuleMethod3()
        {
            if (GelenAksesuar >= SiparisEdilen&&SiparisEdilen!=0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override void OnSaving()
        {
            if (!(Session is NestedUnitOfWork) && Session.DataLayer != null && Session.IsNewObject(this) && string.IsNullOrEmpty(TalepNo))
            {
                int deger = DistributedIdGeneratorHelper.Generate(Session.DataLayer, this.GetType().FullName, "MyServerPrefix");
                TalepNo = string.Format("{0:D6}", deger);
                FireOran = 5;
            }
            RuleMethod();
            RuleMethod2();
            RuleMethod3();
            base.OnSaving();    
        }
        
        double kmsfr;
        decimal kur;
        decimal s_kur;
        double pln;
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            try
            {
                if (Aksesuar != null&&IsDeleted==false)
                {
                  
                     kmsfr = (fAdet / 100) * FireOran;
                   
                    pln = kmsfr + fAdet;
                    Planlanan = Math.Ceiling(double.Parse(pln.ToString()));
                    T_Mlyt_Dvz = Planlanan * M_Fiyat;
                    M_FiyatToplam = T_Mlyt_Dvz *double.Parse( M_Kur.ToString());
                }
            }
            catch (Exception)
            {


            }

             kur = 0;
            if (propertyName == "M_KurTarih" || propertyName == "M_ParaBirimi")
            {
                try
                {
                    string bugun = "https://www.tcmb.gov.tr/kurlar/today.xml";
                    string yil = M_KurTarih.ToString("yyyy");
                    string ay = M_KurTarih.ToString("MM");
                    string gun = M_KurTarih.ToString("dd");
                    string trh = yil + ay + "/" + gun + ay + yil;
                    string eskitarih = "https://www.tcmb.gov.tr/kurlar/" + trh + ".xml";
                    var xmldosya = new XmlDocument();

                    if (trh != "000101/01010001")
                    {


                        if (M_KurTarih.Date == DateTime.Today)
                        {
                            xmldosya.Load(bugun);
                        }
                        else
                        {
                            xmldosya.Load(eskitarih);
                        }
                        if (M_ParaBirimi.P_Birimi == "USD")
                        {
                            string dolarsatis = xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteBuying").InnerXml;
                            kur = decimal.Parse(dolarsatis, CultureInfo.InvariantCulture);

                        }
                        if (M_ParaBirimi.P_Birimi == "EUR")
                        {
                            string EUROsatis = xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteBuying").InnerXml;
                            kur = decimal.Parse(EUROsatis, CultureInfo.InvariantCulture);
                        }
                        if (M_ParaBirimi.P_Birimi == "GBP")
                        {

                            string sterlnsatis = xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='GBP']/BanknoteBuying").InnerXml;
                            kur = decimal.Parse(sterlnsatis, CultureInfo.InvariantCulture);
                        }
                        if (M_ParaBirimi.P_Birimi == "TL")
                        {


                            kur = decimal.Parse("1");
                        }
                        //String.Format("{0}'C", dolarsatis.ToString());
                        M_Kur = kur;
                    }
                }
                catch (Exception )
                {

                   //MessageBox.Show("Resmi Tatil ve Hafta Sonları Sorgulama Yapılamaz.Lütfen Başka Bir Tarih Seçin");
                }

            }
             s_kur = 0;
            if (propertyName == "S_KurTarihi" || propertyName == "S_ParaBirimi")
            {
                try
                {
                    string bugun = "https://www.tcmb.gov.tr/kurlar/today.xml";
                    string yil = S_KurTarihi.ToString("yyyy");
                    string ay = S_KurTarihi.ToString("MM");
                    string gun = S_KurTarihi.ToString("dd");
                    string trh = yil + ay + "/" + gun + ay + yil;
                    string eskitarih = "https://www.tcmb.gov.tr/kurlar/" + trh + ".xml";
                    var xmldosya = new XmlDocument();

                    if (trh != "000101/01010001")
                    {


                        if (S_KurTarihi.Date == DateTime.Today)
                        {
                            xmldosya.Load(bugun);
                        }
                        else
                        {
                            xmldosya.Load(eskitarih);
                        }
                        if (S_ParaBirimi.P_Birimi == "USD")
                        {
                            string dolarsatis = xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='USD']/BanknoteBuying").InnerXml;
                            s_kur = decimal.Parse(dolarsatis, CultureInfo.InvariantCulture);

                        }
                        if (S_ParaBirimi.P_Birimi == "EUR")
                        {
                            string EUROsatis = xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='EUR']/BanknoteBuying").InnerXml;
                            s_kur = decimal.Parse(EUROsatis, CultureInfo.InvariantCulture);
                        }
                        if (S_ParaBirimi.P_Birimi == "GBP")
                        {

                            string sterlnsatis = xmldosya.SelectSingleNode("Tarih_Date/Currency[@Kod='GBP']/BanknoteBuying").InnerXml;
                            s_kur = decimal.Parse(sterlnsatis, CultureInfo.InvariantCulture);
                        }
                        if (S_ParaBirimi.P_Birimi == "TL")
                        {


                            s_kur = decimal.Parse("1");
                        }
                        //String.Format("{0}'C", dolarsatis.ToString());
                        S_Kuru = s_kur;
                    }
                }
                catch (Exception )
                {

                    //System.Windows.Forms.MessageBox.Show("Resmi Tatil ve Hafta Sonları Sorgulama Yapılamaz.Lütfen Başka Bir Tarih Seçin");
                }

            }
            

            //if (SiparisKarti != null)
            //{
            //    try
            //    {
            //        if (SiparisKarti.AksesuarSiparisCollection.Count > 0)
            //        {

            //            SiparisKarti.P_MalzemeTL= SiparisKarti.AksesuarSiparisCollection.Sum(x => x.M_FiyatToplam);
            //        }
            //    }
            //    catch (Exception)
            //    {


            //    }

            //}
            base.OnChanged(propertyName, oldValue, newValue);
        }
    }

}
