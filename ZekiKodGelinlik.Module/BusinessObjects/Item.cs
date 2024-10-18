using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZekiKodGelinlik.Module.BusinessObjects
{
    [DefaultClassOptions]
    [DeferredDeletion(false)]
    [ImageName("BO_Product")]
    [DefaultProperty("Code")]
    [RuleCombinationOfPropertiesIsUnique(
        DefaultContexts.Save, 
        "Code;Barcode", 
        CustomMessageTemplate = "The combination of 'Code' and 'Barcode' must be unique!")]

    public class Product : BaseObject
    {
        public Product(Session session) : base(session) { }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
        }

        /// <summary>
        /// Product's unique code
        /// </summary>
        private string code;
        [Size(100)]
        [RuleUniqueValue]
        [RuleRequiredField]
        public string Code
        {
            get { return code; }
            set { SetPropertyValue(nameof(Code), ref code, value); }
        }

        /// <summary>
        /// Product's unique barcode
        /// </summary>
        private string barcode;
        public string Barcode
        {
            get { return barcode; }
            set { SetPropertyValue(nameof(Barcode), ref barcode, value); }
        }

        /// <summary>
        /// Product's price
        /// </summary>
        private decimal price;
        [ModelDefault("DisplayFormat", "€ {0:0.00}")]
        public decimal Price
        {
            get { return price; }
            set { SetPropertyValue(nameof(Price), ref price, value); }
        }

        /// <summary>
        /// Stock quantity
        /// </summary>
        private int quantity;
        public int Quantity
        {
            get { return quantity; }
            set { SetPropertyValue(nameof(Quantity), ref quantity, value); }
        }


    }
}
