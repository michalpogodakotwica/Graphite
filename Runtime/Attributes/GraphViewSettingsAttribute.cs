using System;

namespace Attributes
{
    public class GraphViewSettingsAttribute : Attribute
    {
        public readonly string Title;

        public readonly bool EnableMinimap;
        public readonly bool EnableGrid;
        public readonly Type[] OutputsOnRight;
        
        public readonly string[] GraphStyleSheetsResourcePaths;

        public GraphViewSettingsAttribute(
            string title = "GraphView",
            bool enableMinimap = true, 
            bool enableGrid  = true,
            Type[] outputsOnRight = null,
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