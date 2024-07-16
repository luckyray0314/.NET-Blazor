using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System;
using System.Linq;

namespace Develoop.Forms.Module.BusinessObjects.Moduli
{
    [DefaultClassOptions]
    [NavigationItem("Moduli")]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("DisplayMemberNameForLookupEditorsOfThisType")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    //[Persistent("DatabaseTableName")]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class CategoriaModulo : BaseObject
    {
        private string _nome;

        public CategoriaModulo(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
        }

        [Size(1000)]
        [RuleRequiredField]
        public string Nome { get { return _nome; } set { SetPropertyValue(nameof(Nome), ref _nome, value); } }

        [Association("Moduli-Categoria")]
        public XPCollection<Modulo> Moduli
        {
            get { return GetCollection<Modulo>(nameof(Moduli)); }
        }
    }
}