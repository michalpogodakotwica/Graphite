using System;

namespace Graphite.Runtime.Attributes
{
    public class GraphViewSettingsAttribute : Attribute
    {
        public readonly string Title;

        public readonly bool EnableMinimap;
        public readonly bool EnableGrid;
        public readonly bool OutputsOnRight;
        
        public readonly string[] GraphStyleSheetsResourcePaths;

        public GraphViewSettingsAttribute(
            string title = "GraphView",
            bool enableMinimap = true, 
            bool enableGrid  = true,
            bool outputsOnRight  = true,
            params string[] graphStyleSheetsResourcePaths)
        {
             Title = title;
             EnableMinimap = enableMinimap;
             EnableGrid = enableGrid;
             OutputsOnRight = outputsOnRight;
             GraphStyleSheetsResourcePaths = graphStyleSheetsResourcePaths;
        }
    }
}