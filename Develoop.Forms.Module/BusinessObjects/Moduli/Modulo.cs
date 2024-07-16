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
    public class Modulo : BaseObject
    {
        private string _nome;
        private string _json;
        private CategoriaModulo categoria;
        public Modulo(Session session) : base(session)
        {
        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
            Json = "{}";
        }

        [Size(1000)]
        [RuleRequiredField]
        public string Nome { get { return _nome; } set { SetPropertyValue(nameof(Nome), ref _nome, value); } }

        [Association("Moduli-Categoria")]
        public CategoriaModulo Categoria
        {
            get { return categoria; }
            set { SetPropertyValue(nameof(Categoria), ref categoria, value); }
        }

        [Size(SizeAttribute.Unlimited)]
        public string Json { get { return _json; } set { SetPropertyValue(nameof(Json), ref _json, value); } }
    }
}