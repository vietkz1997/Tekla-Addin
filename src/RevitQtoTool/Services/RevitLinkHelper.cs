using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitQtoTool.Services
{
    public class LinkDocumentInfo
    {
        public string DocumentTitle { get; set; } = string.Empty;
        public Document LinkDoc { get; set; }
        public Transform TotalTransform { get; set; } = Transform.Identity;
    }

    public static class RevitLinkHelper
    {
        public static List<LinkDocumentInfo> GetLoadedLinks(Document hostDoc)
        {
            var links = new List<LinkDocumentInfo>();
            if (hostDoc == null) return links;

            var linkCollector = new FilteredElementCollector(hostDoc)
                .OfClass(typeof(RevitLinkInstance))
                .ToElements()
                .Cast<RevitLinkInstance>();

            foreach (var linkInst in linkCollector)
            {
                Document linkDoc = linkInst.GetLinkDocument();
                if (linkDoc != null && linkDoc.IsValidObject)
                {
                    links.Add(new LinkDocumentInfo
                    {
                        DocumentTitle = linkDoc.Title,
                        LinkDoc = linkDoc,
                        TotalTransform = linkInst.GetTotalTransform()
                    });
                }
            }

            return links;
        }
    }
}
