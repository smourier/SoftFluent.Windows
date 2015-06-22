using System;
using System.Windows;
using System.Windows.Controls;
using SoftFluent.Windows.Utilities;

namespace SoftFluent.Windows
{
    public class PropertyGridOptionsAttribute : Attribute
    {
        public string[] EnumNames { get; set; }
        public object[] EnumValues { get; set; }
        public bool IsEnum { get; set; }
        public bool IsFlagsEnum { get; set; }
        public bool CollectionEditorHasOnlyOneColumn { get; set; }
        public int SortOrder { get; set; }
        public string EditorDataTemplatePropertyPath { get; set; }
        public string EditorDataTemplateSelectorPropertyPath { get; set; }
        public Type EditorType { get; set; }
        public string EditorResourceKey { get; set; }
        public object EditorDataTemplateResourceKey { get; set; }
        public Type PropertyType { get; set; }
        public bool ForceReadWrite { get; set; }

        public static DataTemplate SelectTemplate(PropertyGridProperty property, object item, DependencyObject container)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            PropertyGridOptionsAttribute att = FromProperty(property);
            if (att == null)
                return null;

            if (att.EditorDataTemplateResourceKey != null)
            {
                if (Application.Current != null)
                {
                    DataTemplate dt = (DataTemplate)Application.Current.TryFindResource(att.EditorDataTemplateResourceKey);
                    if (dt != null)
                        return dt;
                }

                FrameworkElement fe = container as FrameworkElement;
                if (fe != null)
                {
                    DataTemplate dt = (DataTemplate)fe.TryFindResource(att.EditorDataTemplateResourceKey);
                    if (dt != null)
                        return dt;
                }

                return null;
            }

            if (att.EditorType != null)
            {
                object editor = Activator.CreateInstance(att.EditorType);
                if (att.EditorDataTemplateSelectorPropertyPath != null)
                {
                    DataTemplateSelector dts = (DataTemplateSelector)DataBindingEvaluator.GetPropertyValue(editor, att.EditorDataTemplateSelectorPropertyPath);
                    return dts != null ? dts.SelectTemplate(item, container) : null;
                }

                if (att.EditorDataTemplatePropertyPath != null)
                    return (DataTemplate)DataBindingEvaluator.GetPropertyValue(editor, att.EditorDataTemplatePropertyPath);

                ContentControl cc = editor as ContentControl;
                if (cc != null)
                {
                    if (cc.ContentTemplateSelector != null)
                    {
                        DataTemplate template = cc.ContentTemplateSelector.SelectTemplate(item, container);
                        if (template != null)
                            return template;
                    }

                    return cc.ContentTemplate;
                }

                ContentPresenter cp = editor as ContentPresenter;
                if (cp != null)
                {
                    if (cp.ContentTemplateSelector != null)
                    {
                        DataTemplate template = cp.ContentTemplateSelector.SelectTemplate(item, container);
                        if (template != null)
                            return template;
                    }

                    return cp.ContentTemplate;
                }
            }
            return null;
        }

        public static PropertyGridOptionsAttribute FromProperty(PropertyGridProperty property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            PropertyGridOptionsAttribute att = property.Options;
            if (att != null)
                return att;

            if (property.Descriptor != null)
            {
                att = AssemblyUtilities.GetAttribute<PropertyGridOptionsAttribute>(property.Descriptor);
            }

            if (att == null)
            {
                if (property.PropertyType == typeof(bool))
                {
                    att = new PropertyGridOptionsAttribute();
                    att.IsEnum = true;
                    att.EnumNames = new[] { "Yes", "No" };
                    att.EnumValues = new object[] { true, false };
                }
                else if (property.PropertyType == typeof(bool?))
                {
                    att = new PropertyGridOptionsAttribute();
                    att.IsEnum = true;
                    att.EnumNames = new[] { "Yes", "No", "" };
                    att.EnumValues = new object[] { true, false, null };
                }
            }
            return att;
        }
    }
}