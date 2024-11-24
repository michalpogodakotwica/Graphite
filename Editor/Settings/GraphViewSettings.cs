using System.Linq;
using Graphite.Attributes;
using Graphite.Editor.Attributes;
using Graphite.Editor.CompatiblePortsProvider;
using Graphite.Editor.CopyPasteHandler;
using Graphite.Editor.CreateNodeSearchWindowProvider;
using Graphite.Editor.ElementDrawerProvider;
using Graphite.Editor.GraphDrawer;
using Graphite.Editor.GraphDrawer.NodeDrawers;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Graphite.Editor.Settings
{
    public class GraphViewSettings
    {
        public DisplaySettings DisplaySettings;
        
        public IElementDrawerMapping NodeDrawerTypeMapping;
        public IElementDrawerMapping OutputDrawerTypeMapping;
        public IElementDrawerMapping InputDrawerTypeMapping;

        public MiniMapSettings MiniMapSettings;
        public ICopyPasteHandler CopyPasteHandler;
        public ICompatiblePortsProvider CompatiblePortsProvider;
        public ICreateNodeSearchTreeProvider CreateNodeSearchTreeProvider;

        public GraphViewSettings(SerializedProperty graphProperty, IGraph graph)
        {
            NodeDrawerTypeMapping = new DrawerTypeMapping<INode, NodeDrawer, CustomNodeDrawerAttribute>();
            OutputDrawerTypeMapping = new DrawerTypeMapping<IOutput, OutputDrawer, CustomOutputDrawerAttribute>();
            InputDrawerTypeMapping = new DrawerTypeMapping<IInput, InputDrawer, CustomInputDrawerAttribute>();
            
            DisplaySettings = new DisplaySettings(graphProperty);
            MiniMapSettings = new MiniMapSettings();
            CopyPasteHandler = new DefaultCopyPasteHandler();
            CompatiblePortsProvider = new DefaultPortsProvider();
            CreateNodeSearchTreeProvider = new DefaultCreateNodeSearchTreeProvider();
        }

        public GraphViewSettings(SerializedProperty graphProperty, GraphViewSettingsAttribute attribute)
        {
            NodeDrawerTypeMapping = new DrawerTypeMapping<INode, NodeDrawer, CustomNodeDrawerAttribute>();
            OutputDrawerTypeMapping = new DrawerTypeMapping<IOutput, OutputDrawer, CustomOutputDrawerAttribute>();
            InputDrawerTypeMapping = new DrawerTypeMapping<IInput, InputDrawer, CustomInputDrawerAttribute>();
            
            DisplaySettings = new DisplaySettings(graphProperty, attribute);
            MiniMapSettings = new MiniMapSettings(attribute);
            CopyPasteHandler = new DefaultCopyPasteHandler();
            CompatiblePortsProvider = new DefaultPortsProvider();
            CreateNodeSearchTreeProvider = new DefaultCreateNodeSearchTreeProvider();
        }
    }

    public class DisplaySettings
    {
        public string Title;
        public bool EnableGrid;
        public StyleSheet[] GraphStyleSheets;
        public ScaleSettings ScaleSettings;
        public bool ReverseConnectionFlow;

        public DisplaySettings(SerializedProperty graphProperty)
        {
            Title = graphProperty.serializedObject.targetObject.name + "/" + graphProperty.serializedObject.targetObject.GetType().Name + "/" + graphProperty.displayName;

            GraphStyleSheets = new[] { Resources.Load<StyleSheet>("GraphView") };
            
            EnableGrid = true;
            ReverseConnectionFlow = false;
            ScaleSettings = new ScaleSettings();
        }

        public DisplaySettings(SerializedProperty graphProperty, GraphViewSettingsAttribute attribute)
        {
            Title = graphProperty.serializedObject.targetObject.name + "/" + graphProperty.serializedObject.targetObject.GetType().Name + "/" + graphProperty.displayName;

            GraphStyleSheets = new[] { Resources.Load<StyleSheet>("GraphView") }
                .Concat(attribute.GraphStyleSheetsResourcePaths.Select(Resources.Load<StyleSheet>))
                .Where(styleSheet => styleSheet != null)
                .ToArray();

            ReverseConnectionFlow = attribute.ReverseConnectionFlow;
            EnableGrid = attribute.EnableGrid;
            ScaleSettings = new ScaleSettings();
        }
    }

    public class ScaleSettings
    {
        public float MinScale;
        public float MaxScale;
        public float ScaleStep;
        public float ReferenceScale;

        public ScaleSettings()
        {
            MinScale = ContentZoomer.DefaultMinScale;
            MaxScale = ContentZoomer.DefaultMaxScale;
            ScaleStep = ContentZoomer.DefaultScaleStep;
            ReferenceScale = ContentZoomer.DefaultReferenceScale;
        }
    }
    
    public class MiniMapSettings
    {
        public bool EnableMinimap;

        public MiniMapSettings()
        {
            EnableMinimap = true;
        }
        
        public MiniMapSettings(GraphViewSettingsAttribute attribute)
        {
            EnableMinimap = attribute.EnableMinimap;
        }
    }
}