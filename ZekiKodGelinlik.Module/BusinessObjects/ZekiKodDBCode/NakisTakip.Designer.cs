﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class NakisTakip : XPObject
    {
        Sirketiz.Module.BusinessObjects.Sirket_izDB.kisi_kartlari fNakisciAdSoyad;
        public Sirketiz.Module.BusinessObjects.Sirket_izDB.kisi_kartlari NakisciAdSoyad
        {
            get { return fNakisciAdSoyad; }
            set { SetPropertyValue<Sirketiz.Module.BusinessObjects.Sirket_izDB.kisi_kartlari>(nameof(NakisciAdSoyad), ref fNakisciAdSoyad, value); }
        }
        SiparisKarti fSiparisKarti;
        public SiparisKarti SiparisKarti
        {
            get { return fSiparisKarti; }
            set { SetPropertyValue<SiparisKarti>(nameof(SiparisKarti), ref fSiparisKarti, value); }
        }
        DateTime fVerilenTarih;
        public DateTime VerilenTarih
        {
            get { return fVerilenTarih; }
            set { SetPropertyValue<DateTime>(nameof(VerilenTarih), ref fVerilenTarih, value); }
        }
        DateTime fTerminTarihi;
        public DateTime TerminTarihi
        {
            get { return fTerminTarihi; }
            set { SetPropertyValue<DateTime>(nameof(TerminTarihi), ref fTerminTarihi, value); }
        }
        DateTime fGelenTarih;
        public DateTime GelenTarih
        {
            get { return fGelenTarih; }
            set { SetPropertyValue<DateTime>(nameof(GelenTarih), ref fGelenTarih, value); }
        }
        decimal fOnerilenUcret;
        public decimal OnerilenUcret
        {
            get { return fOnerilenUcret; }
            set { SetPropertyValue<decimal>(nameof(OnerilenUcret), ref fOnerilenUcret, value); }
        }
        decimal fAnlasilanUcret;
        public decimal AnlasilanUcret
        {
            get { return fAnlasilanUcret; }
            set { SetPropertyValue<decimal>(nameof(AnlasilanUcret), ref fAnlasilanUcret, value); }
        }
        string fAciklama;
        [Size(SizeAttribute.Unlimited)]
        public string Aciklama
        {
            get { return fAciklama; }
            set { SetPropertyValue<string>(nameof(Aciklama), ref fAciklama, value); }
        }
    }

}
