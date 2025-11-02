using System;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.IO;

namespace ZekiKod.Module.BusinessObjects.ZekiKodDB
{

    public partial class SiparisEkResimler
    {
        public SiparisEkResimler(Session session) : base(session) { }
        public override void AfterConstruction() { base.AfterConstruction(); }

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (propertyName == nameof(Resim) && newValue != null)
            {
                ResimThumb = GenerateThumbnail((byte[])newValue);
            }
        }

        private byte[] GenerateThumbnail(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0)
                return null;

            using (var image = Image.Load(imageBytes))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(150, 150),
                    Mode = ResizeMode.Max
                }));

                using (var ms = new MemoryStream())
                {
                    image.SaveAsJpeg(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
