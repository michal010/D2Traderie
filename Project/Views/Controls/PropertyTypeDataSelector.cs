using D2Traderie.Project.Models;
using System.Windows;
using System.Windows.Controls;

namespace D2Traderie
{
    public class PropertyTypeDataSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object selector, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            PropertyEntity propertyEntity = selector as PropertyEntity;

            switch(propertyEntity.GetPropertyType())
            {
                case PropertyType.numberType:
                    return element.FindResource("PropertyNumberTemplate") as DataTemplate;
                case PropertyType.booleanType:
                    return element.FindResource("PropertyBooleanTemplate") as DataTemplate;
                case PropertyType.stringType:
                    return element.FindResource("PropertyStringTemplate") as DataTemplate;
            }
            
            return element.FindResource("PropertyBooleanTemplate") as DataTemplate;
        }
    }
}
