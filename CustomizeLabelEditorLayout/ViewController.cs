using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CoreGraphics;
using Syncfusion.iOS.DataForm;
using Syncfusion.iOS.DataForm.Editors;
using UIKit;

namespace CustomizeLabelEditorLayout
{
    public partial class ViewController : UIViewController
    {

        Syncfusion.iOS.DataForm.SfDataForm dataForm;
        protected ViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view, typically from a nib.

            dataForm = new SfDataForm();//new CoreGraphics.CGRect(10, 100, this.View.Bounds.Width, this.View.Bounds.Height));

            dataForm.LayoutManager = new DataFormLayoutManagerExt(dataForm);
            dataForm.RegisterEditor("ImageEditor", new CustomEditor(this.dataForm));
            dataForm.RegisterEditor("Image", "ImageEditor");
            dataForm.DataObject = new ContactInfo();
            dataForm.BackgroundColor = UIColor.White;

            this.View = dataForm;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }

    

    public class CustomEditor : DataFormEditor<UIImageView>
    {
        public CustomEditor(SfDataForm dataForm) : base(dataForm)
        {

        }
        protected override UIImageView OnCreateEditorView()
        {
            var imageView = new UIImageView(UIImage.FromBundle("Person.png"));
           
            return imageView;

        }
        protected override void OnInitializeView(DataFormItem dataFormItem, UIImageView view)
        {

            base.OnInitializeView(dataFormItem, view);
        }
		protected override void OnCommitValue(UIImageView view)
		{
            base.OnCommitValue(view);
		}
	}

    public class DataFormLayoutManagerExt : DataFormLayoutManager
    {
        public DataFormLayoutManagerExt(SfDataForm dataForm) : base(dataForm)
        {

        }
        protected override DataFormItemView CreateDataFormItemView(int rowIndex, int columnIndex)
        {

            var dataFormItemBase = this.DataForm.ItemManager.DataFormItems[rowIndex, columnIndex];
            if (dataFormItemBase == null)
                return null;

            var dataFormItem = dataFormItemBase as DataFormItem;
            if (dataFormItem == null)
                return null;
            if (dataFormItem.Name == "Image")
            {
                DataFormEditorBase editor = null;
                var defaultEditors = this.DataForm.ItemManager.GetType().GetProperty("DefaultEditors", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(this.DataForm.ItemManager);
                var defaultDataFormEditors = (defaultEditors as Dictionary<string, DataFormEditorBase>);
                var value = defaultDataFormEditors.TryGetValue(dataFormItem.Editor, out editor);
                if (!value)
                {
                    editor = defaultDataFormEditors["Text"];
                }

                var dataFormItemView = new DataFormItemViewExt();

                ReflectionHelper.SetValue(dataFormItemView, "DataFormItem", dataFormItem);
                ReflectionHelper.SetValue(dataFormItem, "View", dataFormItemView);

                ReflectionHelper.SetValue(dataFormItemView, "Editor", editor);

                var label = this.GenerateViewForLabel(dataFormItem);

                //if (label != null)
                    //dataFormItemView.AddSubview(label);
                var editorMethod = ReflectionHelper.GetMethod(this.GetType(), "GenerateViewForEditor");
                var editorview = ReflectionHelper.InVoke(editorMethod, this, new object[] { dataFormItemView.Editor, dataFormItem });
                ReflectionHelper.SetValue(dataFormItemView, "EditorView", editorview);

                if ((editorview as UIView).Superview == null)
                    dataFormItemView.AddSubview(editorview as UIView);
                dataFormItemView.Hidden = !dataFormItem.IsVisible;
                return dataFormItemView;
            }
            else

                return base.CreateDataFormItemView(rowIndex, columnIndex);
        }
    }

    public class DataFormItemViewExt : DataFormItemView
    {

        public DataFormItemViewExt()
        {

        }
        public override void LayoutSubviews()
        {
            var dataform = ReflectionHelper.GetValue(this.Editor, "DataForm");
            var sfdataform = dataform as SfDataForm;
        
            if (this.Superview != null)
            {
                var scrollPanel = ReflectionHelper.GetScrollPanel(sfdataform) as ScrollPanel;

                nfloat ypos = 0;
                CGRect frame = new CGRect(0, ypos, 250, sfdataform.LabelPosition == LabelPosition.Top ? Convert.ToDouble(ReflectionHelper.GetValue(scrollPanel, "LabelHeight")) : Convert.ToDouble(ReflectionHelper.GetValue(scrollPanel, "RowHeight")));
                var layotlabel = ReflectionHelper.GetMethod(sfdataform.LayoutManager.GetType(), "LayoutLabel");
                ReflectionHelper.InVoke(layotlabel, sfdataform.LayoutManager, new object[] { this, frame });

                if (sfdataform.LabelPosition == LabelPosition.Top && DataFormItem.ShowLabel)
                    ypos += (nfloat)ReflectionHelper.GetValue(scrollPanel, "LabelHeight");

                // Set YPos - it will give some space in Top of Image

               var rowspan= this.DataFormItem.GetType().GetProperty("RowSpan", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
               var rowspanvalue =  rowspan.GetValue(this.DataFormItem);
                frame = new CGRect(0, ypos + 50, 250, Convert.ToDouble(ReflectionHelper.GetValue(scrollPanel, "RowHeight")) * Convert.ToDouble(rowspanvalue));

                var layouteditor = ReflectionHelper.GetMethod(sfdataform.LayoutManager.GetType(), "LayoutEditor");
                ReflectionHelper.InVoke(layouteditor, sfdataform.LayoutManager, new object[] { this, frame });

                ypos += (nfloat)Convert.ToDouble(ReflectionHelper.GetValue(scrollPanel, "RowHeight")) * (nfloat)Convert.ToDouble(rowspanvalue);
            }
        }
    }



    internal static class ReflectionHelper
    {
        internal static MethodInfo GetMethod(Type type, string name, int argumentsCount)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                if (method.Name.Equals(name))
                {
                    if (method.GetParameters().Count() == argumentsCount)
                        return method;
                }
            }
            return null;
        }
        internal static MethodInfo GetMethod(Type type, string name)
        {
            return type.GetMethod(name,  System.Reflection.BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance );
        }
        public static ScrollPanel GetScrollPanel(this SfDataForm dataForm)
        {
            return ReflectionHelper.GetValue(dataForm, "ScrollPanel") as ScrollPanel;
        }

        internal static void InvokeMethod(this object instance, string methodName, object[] arguments = null)
        {
            try
            {
                instance.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, arguments);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        internal static PropertyInfo GetProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | BindingFlags.Public);
        }

        internal static FieldInfo GetField(Type type, string fieldName)
        {
            return type.GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | BindingFlags.Public);
        }

        internal static object GetValue(object obj, string propertyName)
        {
            var propertyInfo = GetProperty(obj.GetType(), propertyName);
            if (propertyInfo == null)
            {
                var fieldInfo = GetField(obj.GetType(), propertyName);
                return fieldInfo.GetValue(obj);
            }
            return propertyInfo.GetValue(obj);
        }

        internal static void SetValue(object obj, string propertyName, object value)
        {
            var propertyInfo = GetProperty(obj.GetType(), propertyName);
            if (propertyInfo == null)
            {
                var fieldInfo = GetField(obj.GetType(), propertyName);
                fieldInfo.SetValue(obj, value);
                return;
            }
            propertyInfo.SetValue(obj, value);
        }

        internal static object InVoke(MethodInfo method, object obj, object[] parameters)
        {
            return method.Invoke(obj, parameters);
        }

    }
}

