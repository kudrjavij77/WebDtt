using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace WebDtt.App_Start
{
    public class DevExtremeBundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            var scriptBundle = new ScriptBundle("~/Scripts/DevExtremeBundle");
            var styleBundle = new StyleBundle("~/Content/DevExtremeBundle");
            // CLDR scripts
            scriptBundle
                .Include("~/Scripts/cldr.js")
                .Include("~/Scripts/cldr/event.js")
                .Include("~/Scripts/cldr/supplemental.js")
                .Include("~/Scripts/cldr/unresolved.js");
            // Globalize 1.x
            scriptBundle
                .Include("~/Scripts/globalize.js")
                .Include("~/Scripts/globalize/message.js")
                .Include("~/Scripts/globalize/number.js")
                .Include("~/Scripts/globalize/currency.js")
                .Include("~/Scripts/globalize/date.js");
            // NOTE: jQuery may already be included in the default script bundle. Check the BundleConfig.cs file​​​
            // scriptBundle
            //    .Include("~/Scripts/jquery-1.10.2.js");
            // JSZip for client-side exporting
            // scriptBundle
            //    .Include("~/Scripts/jszip.js");dfgyher
            // DevExtreme + extensions
            scriptBundle
                .Include("~/Scripts/dx-quill.js")
                .Include("~/Scripts/dx.all.js")
                .Include("~/Scripts/aspnet/dx.aspnet.data.js")
                .Include("~/Scripts/dx.aspnet.mvc.js");
            // VectorMap data
            // scriptBundle
            //    .Include("~/Scripts/vectormap-data/africa.js")
            //    .Include("~/Scripts/vectormap-data/canada.js")
            //    .Include("~/Scripts/vectormap-data/eurasia.js")
            //    .Include("~/Scripts/vectormap-data/europe.js")
            //    .Include("~/Scripts/vectormap-data/usa.js")
            //    .Include("~/Scripts/vectormap-data/world.js");
            // DevExtreme themes              
            styleBundle
                .Include("~/Content/dx.common.css")
                .Include("~/Content/dx.light.css");
            bundles.Add(scriptBundle);
            bundles.Add(styleBundle);
#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}