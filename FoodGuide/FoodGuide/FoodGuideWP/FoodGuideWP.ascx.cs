using System;
using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;
using System.Linq;
using System.Web;
using System.Text;

namespace FoodGuide.FoodGuideWP
{
    [ToolboxItemAttribute(false)]
    public partial class FoodGuideWP : WebPart
    {
        // Uncomment the following SecurityPermission attribute only when doing Performance Profiling on a farm solution
        // using the Instrumentation method, and then remove the SecurityPermission attribute when the code is ready
        // for production. Because the SecurityPermission attribute bypasses the security check for callers of
        // your constructor, it's not recommended for production purposes.
        // [System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Assert, UnmanagedCode = true)]
        public FoodGuideWP()
        {
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            InitializeControl();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            using (FoodguideDataContext foodGuideContext = new FoodguideDataContext(SPContext.Current.Web.Url))
            {
                var strBuilder = new StringBuilder();
                foreach (var item in foodGuideContext.VisitedPlaces)
                {
                    strBuilder.AppendLine(item.Title + item.Id);
                }
                this.Controls.Add(new System.Web.UI.LiteralControl(strBuilder.ToString()));
            }
        }
    }
}
