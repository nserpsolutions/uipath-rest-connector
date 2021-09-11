using System.Activities.Presentation.Metadata;
using System.ComponentModel;
using System.ComponentModel.Design;
using NetSuiteRestletConnector.Activities.Design.Designers;
using NetSuiteRestletConnector.Activities.Design.Properties;

namespace NetSuiteRestletConnector.Activities.Design
{
    public class DesignerMetadata : IRegisterMetadata
    {
        public void Register()
        {
            var builder = new AttributeTableBuilder();
            builder.ValidateTable();

            var categoryAttribute = new CategoryAttribute($"{Resources.Category}");

            builder.AddCustomAttributes(typeof(NetSuiteProcessRestlet), categoryAttribute);
            builder.AddCustomAttributes(typeof(NetSuiteProcessRestlet), new DesignerAttribute(typeof(NetSuiteProcessRestletDesigner)));
            builder.AddCustomAttributes(typeof(NetSuiteProcessRestlet), new HelpKeywordAttribute(""));


            MetadataStore.AddAttributeTable(builder.CreateTable());
        }
    }
}
