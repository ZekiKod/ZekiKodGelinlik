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
namespace Sirketiz.Module.BusinessObjects.Sirket_izDB
{

    [Persistent(@"Agi Tablosu")]
    public partial class agi_tablosu : XPObject
    {
        DateTime fyil;
        public DateTime yil
        {
            get { return fyil; }
            set { SetPropertyValue<DateTime>(nameof(yil), ref fyil, value); }
        }
        string fmedeni_hal;
        public string medeni_hal
        {
            get { return fmedeni_hal; }
            set { SetPropertyValue<string>(nameof(medeni_hal), ref fmedeni_hal, value); }
        }
        short fcocuk_sayisi;
        public short cocuk_sayisi
        {
            get { return fcocuk_sayisi; }
            set { SetPropertyValue<short>(nameof(cocuk_sayisi), ref fcocuk_sayisi, value); }
        }
        kisi_kartlari fkisi_kartlari_to;
        public kisi_kartlari kisi_kartlari_to
        {
            get { return fkisi_kartlari_to; }
            set { SetPropertyValue<kisi_kartlari>(nameof(kisi_kartlari_to), ref fkisi_kartlari_to, value); }
        }
    }

}
