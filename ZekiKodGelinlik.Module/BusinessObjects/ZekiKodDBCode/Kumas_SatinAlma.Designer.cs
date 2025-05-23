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

    public partial class Kumas_SatinAlma : XPObject
    {
        SiparisKarti fSiparisKarti;
        [Association(@"Kumas_SatinAlmaReferencesSiparisKarti")]
        public SiparisKarti SiparisKarti
        {
            get { return fSiparisKarti; }
            set { SetPropertyValue<SiparisKarti>(nameof(SiparisKarti), ref fSiparisKarti, value); }
        }
        DateTime fSiparisTarihi;
        public DateTime SiparisTarihi
        {
            get { return fSiparisTarihi; }
            set { SetPropertyValue<DateTime>(nameof(SiparisTarihi), ref fSiparisTarihi, value); }
        }
        KumasKarti fKumas_Karti;
        public KumasKarti Kumas_Karti
        {
            get { return fKumas_Karti; }
            set { SetPropertyValue<KumasKarti>(nameof(Kumas_Karti), ref fKumas_Karti, value); }
        }
        Renkler fRenk;
        public Renkler Renk
        {
            get { return fRenk; }
            set { SetPropertyValue<Renkler>(nameof(Renk), ref fRenk, value); }
        }
        int fAdet;
        [ColumnDbDefaultValue("0")]
        public int Adet
        {
            get { return fAdet; }
            set { SetPropertyValue<int>(nameof(Adet), ref fAdet, value); }
        }
        int fFireOran;
        [ColumnDbDefaultValue("0")]
        public int FireOran
        {
            get { return fFireOran; }
            set { SetPropertyValue<int>(nameof(FireOran), ref fFireOran, value); }
        }
        double fPlanlanan;
        [ColumnDbDefaultValue("0")]
        public double Planlanan
        {
            get { return fPlanlanan; }
            set { SetPropertyValue<double>(nameof(Planlanan), ref fPlanlanan, value); }
        }
        string fTemsilciAciklama;
        public string TemsilciAciklama
        {
            get { return fTemsilciAciklama; }
            set { SetPropertyValue<string>(nameof(TemsilciAciklama), ref fTemsilciAciklama, value); }
        }
        double fSiparisEdilecek;
        [ColumnDbDefaultValue("0")]
        public double SiparisEdilecek
        {
            get { return fSiparisEdilecek; }
            set { SetPropertyValue<double>(nameof(SiparisEdilecek), ref fSiparisEdilecek, value); }
        }
        Birim fBirimi;
        public Birim Birimi
        {
            get { return fBirimi; }
            set { SetPropertyValue<Birim>(nameof(Birimi), ref fBirimi, value); }
        }
        double fMaliyetFiyat;
        [ColumnDbDefaultValue("0")]
        public double MaliyetFiyat
        {
            get { return fMaliyetFiyat; }
            set { SetPropertyValue<double>(nameof(MaliyetFiyat), ref fMaliyetFiyat, value); }
        }
        double fSatinalmaFiyat;
        [ColumnDbDefaultValue("0")]
        public double SatinalmaFiyat
        {
            get { return fSatinalmaFiyat; }
            set { SetPropertyValue<double>(nameof(SatinalmaFiyat), ref fSatinalmaFiyat, value); }
        }
        Parabirimi fMaliyetParaBirimi;
        public Parabirimi MaliyetParaBirimi
        {
            get { return fMaliyetParaBirimi; }
            set { SetPropertyValue<Parabirimi>(nameof(MaliyetParaBirimi), ref fMaliyetParaBirimi, value); }
        }
        KaliteKontrolTest fTestSonuc;
        public KaliteKontrolTest TestSonuc
        {
            get { return fTestSonuc; }
            set { SetPropertyValue<KaliteKontrolTest>(nameof(TestSonuc), ref fTestSonuc, value); }
        }
        DevExpress.Persistent.BaseImpl.FileData fOrganikBelgesi;
        public DevExpress.Persistent.BaseImpl.FileData OrganikBelgesi
        {
            get { return fOrganikBelgesi; }
            set { SetPropertyValue<DevExpress.Persistent.BaseImpl.FileData>(nameof(OrganikBelgesi), ref fOrganikBelgesi, value); }
        }
        byte[] fKumasSiparisSozlesme;
        [MemberDesignTimeVisibility(true)]
        [DevExpress.Persistent.Base.EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.SpreadsheetPropertyEditor)]
        public byte[] KumasSiparisSozlesme
        {
            get { return fKumasSiparisSozlesme; }
            set { SetPropertyValue<byte[]>(nameof(KumasSiparisSozlesme), ref fKumasSiparisSozlesme, value); }
        }
        string fTalepNo;
        public string TalepNo
        {
            get { return fTalepNo; }
            set { SetPropertyValue<string>(nameof(TalepNo), ref fTalepNo, value); }
        }
        string fVade;
        public string Vade
        {
            get { return fVade; }
            set { SetPropertyValue<string>(nameof(Vade), ref fVade, value); }
        }
        DateTime fTeslimTarihi;
        public DateTime TeslimTarihi
        {
            get { return fTeslimTarihi; }
            set { SetPropertyValue<DateTime>(nameof(TeslimTarihi), ref fTeslimTarihi, value); }
        }
        string fNotlar;
        [Size(SizeAttribute.Unlimited)]
        public string Notlar
        {
            get { return fNotlar; }
            set { SetPropertyValue<string>(nameof(Notlar), ref fNotlar, value); }
        }
        Firmalar fFirma;
        public Firmalar Firma
        {
            get { return fFirma; }
            set { SetPropertyValue<Firmalar>(nameof(Firma), ref fFirma, value); }
        }
        string fFirmaYetkili;
        public string FirmaYetkili
        {
            get { return fFirmaYetkili; }
            set { SetPropertyValue<string>(nameof(FirmaYetkili), ref fFirmaYetkili, value); }
        }
        bool fStokDus;
        public bool StokDus
        {
            get { return fStokDus; }
            set { SetPropertyValue<bool>(nameof(StokDus), ref fStokDus, value); }
        }
        double fGelenKumas;
        public double GelenKumas
        {
            get { return fGelenKumas; }
            set { SetPropertyValue<double>(nameof(GelenKumas), ref fGelenKumas, value); }
        }
        double fStoktakiKumas;
        public double StoktakiKumas
        {
            get { return fStoktakiKumas; }
            set { SetPropertyValue<double>(nameof(StoktakiKumas), ref fStoktakiKumas, value); }
        }
        bool fSiparisVerildi;
        public bool SiparisVerildi
        {
            get { return fSiparisVerildi; }
            set { SetPropertyValue<bool>(nameof(SiparisVerildi), ref fSiparisVerildi, value); }
        }
        string fKumasSatinAlma;
        public string KumasSatinAlma
        {
            get { return fKumasSatinAlma; }
            set { SetPropertyValue<string>(nameof(KumasSatinAlma), ref fKumasSatinAlma, value); }
        }
        string fTopluTalepNo;
        public string TopluTalepNo
        {
            get { return fTopluTalepNo; }
            set { SetPropertyValue<string>(nameof(TopluTalepNo), ref fTopluTalepNo, value); }
        }
        int fEn;
        public int En
        {
            get { return fEn; }
            set { SetPropertyValue<int>(nameof(En), ref fEn, value); }
        }
        int fGramaj;
        public int Gramaj
        {
            get { return fGramaj; }
            set { SetPropertyValue<int>(nameof(Gramaj), ref fGramaj, value); }
        }
        DateTime fMaliyetKurTarih;
        public DateTime MaliyetKurTarih
        {
            get { return fMaliyetKurTarih; }
            set { SetPropertyValue<DateTime>(nameof(MaliyetKurTarih), ref fMaliyetKurTarih, value); }
        }
        DateTime fSatinAlmaKurTarihi;
        public DateTime SatinAlmaKurTarihi
        {
            get { return fSatinAlmaKurTarihi; }
            set { SetPropertyValue<DateTime>(nameof(SatinAlmaKurTarihi), ref fSatinAlmaKurTarihi, value); }
        }
        decimal fMaliyetKuru;
        public decimal MaliyetKuru
        {
            get { return fMaliyetKuru; }
            set { SetPropertyValue<decimal>(nameof(MaliyetKuru), ref fMaliyetKuru, value); }
        }
        decimal fSatinAlmaKuru;
        public decimal SatinAlmaKuru
        {
            get { return fSatinAlmaKuru; }
            set { SetPropertyValue<decimal>(nameof(SatinAlmaKuru), ref fSatinAlmaKuru, value); }
        }
        double fT_Mlyt_Dvz;
        public double T_Mlyt_Dvz
        {
            get { return fT_Mlyt_Dvz; }
            set { SetPropertyValue<double>(nameof(T_Mlyt_Dvz), ref fT_Mlyt_Dvz, value); }
        }
        double fS_Tplm_TL;
        public double S_Tplm_TL
        {
            get { return fS_Tplm_TL; }
            set { SetPropertyValue<double>(nameof(S_Tplm_TL), ref fS_Tplm_TL, value); }
        }
        double fGramaj_M2;
        public double Gramaj_M2
        {
            get { return fGramaj_M2; }
            set { SetPropertyValue<double>(nameof(Gramaj_M2), ref fGramaj_M2, value); }
        }
        string fSatinAlmaAciklama;
        public string SatinAlmaAciklama
        {
            get { return fSatinAlmaAciklama; }
            set { SetPropertyValue<string>(nameof(SatinAlmaAciklama), ref fSatinAlmaAciklama, value); }
        }
        [PersistentAlias("[Planlanan] * [MaliyetFiyat] * [MaliyetKuru]")]
        public double T_Mlyt_TL
        {
            get { return (double)(EvaluateAlias(nameof(T_Mlyt_TL))); }
        }
        string fPantone;
        public string Pantone
        {
            get { return fPantone; }
            set { SetPropertyValue<string>(nameof(Pantone), ref fPantone, value); }
        }
        Parabirimi fS_ParaBirimi;
        public Parabirimi S_ParaBirimi
        {
            get { return fS_ParaBirimi; }
            set { SetPropertyValue<Parabirimi>(nameof(S_ParaBirimi), ref fS_ParaBirimi, value); }
        }
        double fS_Tplm_Dvz;
        public double S_Tplm_Dvz
        {
            get { return fS_Tplm_Dvz; }
            set { SetPropertyValue<double>(nameof(S_Tplm_Dvz), ref fS_Tplm_Dvz, value); }
        }
        [Association(@"Kumas_SatinAlmaReferencesirsaliyeler")]
        public XPCollection<irsaliyeler> irsaliyelers { get { return GetCollection<irsaliyeler>(nameof(irsaliyelers)); } }
        [Association(@"KumasTestDetayReferencesKumas_SatinAlma")]
        public XPCollection<KumasTestDetay> KumasTestDetays { get { return GetCollection<KumasTestDetay>(nameof(KumasTestDetays)); } }
    }

}
