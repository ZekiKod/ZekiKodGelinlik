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

    public partial class SiparisEkDosyalari : XPObject
    {
        string fDosyaAciklama;
        public string DosyaAciklama
        {
            get { return fDosyaAciklama; }
            set { SetPropertyValue<string>(nameof(DosyaAciklama), ref fDosyaAciklama, value); }
        }
        DevExpress.Persistent.BaseImpl.FileData fEkDosyalar;
        public DevExpress.Persistent.BaseImpl.FileData EkDosyalar
        {
            get { return fEkDosyalar; }
            set { SetPropertyValue<DevExpress.Persistent.BaseImpl.FileData>(nameof(EkDosyalar), ref fEkDosyalar, value); }
        }
        SiparisKarti fSiparisKarti;
        [Association(@"SiparisEkDosyalariReferencesSiparisKarti")]
        public SiparisKarti SiparisKarti
        {
            get { return fSiparisKarti; }
            set { SetPropertyValue<SiparisKarti>(nameof(SiparisKarti), ref fSiparisKarti, value); }
        }
    }

}
