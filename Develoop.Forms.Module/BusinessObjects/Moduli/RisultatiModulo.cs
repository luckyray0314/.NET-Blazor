using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace Develoop.Forms.Module.BusinessObjects.Moduli
{
    [DefaultClassOptions]
    [NavigationItem("Moduli")]
    public class RisultatiModulo : BaseObject
    {
        private string _data;
        private string _surveyId;

        public RisultatiModulo(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            Data = "{}";
        }

        [Size(SizeAttribute.Unlimited)]
        public string Data { get { return _data; } set { SetPropertyValue(nameof(Data), ref _data, value); } }

        public string surveyId
        {
            get { return _surveyId; }
            set { SetPropertyValue(nameof(surveyId), ref _surveyId, value); }
        }
    }
}
