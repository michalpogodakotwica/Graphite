using System;

namespace Graphite.Attributes
{
    public class GraphViewSettingsAttribute : Attribute
    {
        public readonly bool ReverseConnectionFlow;
        
        public readonly string[] GraphStyleSheetsResourcePaths;
        
        public readonly bool EnableMinimap;
        public readonly bool EnableGrid;

        public readonly float MinScale;
        public readonly float MaxScale;
        public readonly float ScaleStep;
        
        public GraphViewSettingsAttribute(
            bool enableMinimap = true, 
            bool enableGrid  = true,
            bool reverseConnectionFlow = false,
            float minScale = 0.25f,
            float maxScale = 1f,
            float scaleStep = 0.15f,
            params string[] graphStyleSheetsResourcePaths)
        {
            MinScale = minScale;
            MaxScale = maxScale;
            ScaleStep = scaleStep;
            EnableMinimap = enableMinimap;
             EnableGrid = enableGrid;
             ReverseConnectionFlow = reverseConnectionFlow;
             GraphStyleSheetsResourcePaths = graphStyleSheetsResourcePaths;
        }
    }
}