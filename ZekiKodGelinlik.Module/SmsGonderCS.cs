using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZekiKod.Module.BusinessObjects.ZekiKodDB;

namespace ZekiKodGelinlik.Module
{
    public  class SmsGonderCS
    {
        private string HTTPPoster(string prmSendData)
        {
            try
            {
                WebClient wUpload = new WebClient();
                wUpload.Proxy = null;
                Byte[] bPostArray = Encoding.UTF8.GetBytes(prmSendData);
                Byte[] bResponse = wUpload.UploadData("http://g3.iletimx.com", "POST", bPostArray);
                Char[] sReturnChars = Encoding.UTF8.GetChars(bResponse);
                string sWebPage = new string(sReturnChars);
                return sWebPage;
            }
            catch
            {
                return "-1";
            }
        }

        public void SmsGonder(string kullanici,string Sifre,string smsmesaji,string telefonu,string baslik)
        {
           

                try
                {


                    // Diğer firmalar için olan kod
                    HTTPPoster("<MainmsgBody><UserName>" + kullanici + "-6877" + "</UserName><PassWord>" + Sifre + "</PassWord><Action>12</Action><Mesgbody>" + smsmesaji + "</Mesgbody><Numbers>" + telefonu + "</Numbers><Originator>" + baslik + "</Originator><SDate></SDate></MainmsgBody>");
                }
                catch (Exception ex)
                {
                    
                }
            
        }
    }
}
