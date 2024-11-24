using System;

namespace Graphite.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowInNodeAttribute : Attribute
    {
        public string LabelText = null;
        public bool HideLabel = false;
        public bool IsReadOnly = false;
    }
}