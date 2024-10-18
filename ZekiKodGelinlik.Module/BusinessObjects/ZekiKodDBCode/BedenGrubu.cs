using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using DevExpress.Persistent.Base;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{
    [DefaultClassOptions]
    public partial class BedenGrubu
    {
        public BedenGrubu(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        //protected override void OnSaving()
        //{
        //    string mstr = "";
        //    //if (Musteri != null)
        //    //{
        //    //    mstr = Musteri.MusteriAdi;
        //    //}
        //    //Bedenler = null;
        //    //foreach (var bdnler in Bedenlers)
        //    //{
        //    //    if (bdnler.XXS == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "XXS";
        //    //    }

        //    //    if (bdnler.XS == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "XS";
        //    //    }

        //    //    if (bdnler.S == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "S";
        //    //    }

        //    //    if (bdnler.M == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "M";
        //    //    }


        //    //    if (bdnler.L == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "L";
        //    //    }

        //    //    if (bdnler.XL == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "XL";
        //    //    }

        //    //    if (bdnler.XXL == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "XXL";
        //    //    }

        //    //    if (bdnler.B3XL == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "3XL";
        //    //    }

        //    //    if (bdnler.B4XL == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "4XL";
        //    //    }

        //    //    if (bdnler.B5XL == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "5XL";
        //    //    }

        //    //    if (bdnler.B6XL == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "6XL";
        //    //    }

        //    //    if (bdnler.B4 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "4";
        //    //    }

        //    //    if (bdnler.B6 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "6";
        //    //    }

        //    //    if (bdnler.B8 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "8";
        //    //    }

        //    //    if (bdnler.B10 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "10";
        //    //    }


        //    //    if (bdnler.B12 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "12";
        //    //    }

        //    //    if (bdnler.B14 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "14";
        //    //    }

        //    //    if (bdnler.B16 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "16";
        //    //    }

        //    //    if (bdnler.B18 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "18";
        //    //    }

        //    //    if (bdnler.B20 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "20";
        //    //    }

        //    //    if (bdnler.B22 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "22";
        //    //    }

        //    //    if (bdnler.B24 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "24";
        //    //    }

        //    //    if (bdnler.B26 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "26";
        //    //    }

        //    //    if (bdnler.B28 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "28";
        //    //    }

        //    //    if (bdnler.B30 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "30";
        //    //    }

        //    //    if (bdnler.B32 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "32";
        //    //    }


        //    //    if (bdnler.B34 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "34";
        //    //    }

        //    //    if (bdnler.B36 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "36";
        //    //    }

        //    //    if (bdnler.B56 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "56";
        //    //    }

        //    //    if (bdnler.B62 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "62";
        //    //    }

        //    //    if (bdnler.B68 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "68";
        //    //    }

        //    //    if (bdnler.B74 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "74";
        //    //    }

        //    //    if (bdnler.B80 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "80";
        //    //    }

        //    //    if (bdnler.B86 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "86";
        //    //    }

        //    //    if (bdnler.B92 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "92";
        //    //    }

        //    //    if (bdnler.B98 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "98";
        //    //    }

        //    //    if (bdnler.Y5_6 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "Y5_6";
        //    //    }

        //    //    if (bdnler.Y7_8 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "Y7_8";
        //    //    }

        //    //    if (bdnler.Y9_10 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "Y9_10";
        //    //    }

        //    //    if (bdnler.Y11_12 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "Y11_12";
        //    //    }

        //    //    if (bdnler.Y13 == true)
        //    //    {
        //    //        Bedenler = Bedenler + " " + "Y13";
        //    //    }

        //    //}
            
        //    base.OnSaving();

        //}
        
    }

}
