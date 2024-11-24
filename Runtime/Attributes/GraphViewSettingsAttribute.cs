using System;

namespace Graphite.Attributes
{
    public class GraphViewSettingsAttribute : Attribute
    {
        public readonly bool EnableMinimap;
        public readonly bool EnableGrid;
        public bool ReverseConnectionFlow;
        
        public readonly string[] GraphStyleSheetsResourcePaths;

        public GraphViewSettingsAttribute(
            bool enableMinimap = true, 
            bool enableGrid  = true,
            bool reverseConnectionFlow = false,
            params string[] graphStyleSheetsResourcePaths)
        {
             EnableMinimap = enableMinimap;
             EnableGrid = enableGrid;
             ReverseConnectionFlow = reverseConnectionFlow;
             GraphStyleSheetsResourcePaths = graphStyleSheetsResourcePaths;
        }
    }
}