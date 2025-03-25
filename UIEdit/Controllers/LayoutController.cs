using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using UIEdit.Models;
using UIEdit.Utils;
using UIEdit.Windows;

namespace UIEdit.Controllers
{
    public class LayoutController
    {
        public string SourceText { get; set; }
        public static UIDialog Dialog { get; set; }

        public Exception Parse(string text, string path)
        {
            try
            {
                var tmpFileName = string.Format(@"{0}\tmp.xml", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
                File.WriteAllText(tmpFileName, text);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(tmpFileName);
                Dialog = new UIDialog();

                if (xmlDoc.DocumentElement.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name"))
                    Dialog.Name = xmlDoc.DocumentElement.Attributes["Name"].Value;
                if (xmlDoc.DocumentElement.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width"))
                    Dialog.Width = Convert.ToDouble(xmlDoc.DocumentElement.Attributes["Width"].Value);
                if (xmlDoc.DocumentElement.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height"))
                    Dialog.Height = Convert.ToDouble(xmlDoc.DocumentElement.Attributes["Height"].Value);
                if (xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Resource"))
                {
                    var dlgRes = xmlDoc.DocumentElement.ChildNodes.Cast<XmlNode>().First(t => t.Name == "Resource");
                    if (dlgRes.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        Dialog.FrameImage = string.Format(@"{0}\{1}", path, dlgRes.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage").Attributes["FileName"].Value);
                }
                var xmlDocChilds = xmlDoc.DocumentElement.ChildNodes;

                var childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "EDIT");
                foreach (var element in childNode)
                {
                    var editControl = new UIEditBox();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) editControl.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) editControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) editControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) editControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) editControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            editControl.Hint = textNode.Attributes["String"].Value;
                    }
                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) editControl.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Hint")) editControl.Hint = textNode.Attributes["Hint"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) editControl.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) editControl.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) editControl.FontName = textNode.Attributes["FontName"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                editControl.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.Edits.Add(editControl);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "IMAGEPICTURE");
                foreach (var element in childNode)
                {
                    var imageControl = new UIImagePicture();
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(t => t.Name == "Resource");
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            XmlNode frameImageNode = resourceNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "FrameImage");
                            if (frameImageNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                imageControl.FileName = string.Format(@"{0}\{1}", path, frameImageNode.Attributes["FileName"].Value);
                            if (frameImageNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Frames"))
                                imageControl.Frames = Convert.ToInt32(frameImageNode.Attributes["Frames"].Value);
                        }
                    }

                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) imageControl.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) imageControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) imageControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) imageControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) imageControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            imageControl.Hint = textNode.Attributes["String"].Value;
                    }

                    Dialog.ImagePictures.Add(imageControl);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "SCROLL");
                foreach (var element in childNode)
                {
                    var scrollControl = new UIScroll();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) scrollControl.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) scrollControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) scrollControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) scrollControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) scrollControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(t => t.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "UpImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().FirstOrDefault(t => t.Name == "UpImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                scrollControl.UpImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "DownImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "DownImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                scrollControl.DownImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "ScrollImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "ScrollImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                scrollControl.ScrollImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "BarFrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "BarFrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                scrollControl.BarFrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.Scrolls.Add(scrollControl);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "PROGRESS");
                foreach (var element in childNode)
                {
                    var progressBar = new UIProgressBar();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) progressBar.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) progressBar.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) progressBar.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) progressBar.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) progressBar.Height = Convert.ToDouble(element.Attributes["Height"].Value);

                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(t => t.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                progressBar.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FillImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FillImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                progressBar.FillImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.ProgressBars.Add(progressBar);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "RADIO");
                foreach (var element in childNode)
                {
                    var radioControl = new UIRadioButton();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) radioControl.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) radioControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) radioControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) radioControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) radioControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            radioControl.Hint = textNode.Attributes["String"].Value;
                    }
                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "NormalImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "NormalImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                radioControl.NormalImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        // Not used
                        //if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "CheckedImage"))
                        //{
                        //    var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "CheckedImage");
                        //    if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                        //        radioControl.CheckedImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        //}
                    }
                    Dialog.RadioButtons.Add(radioControl);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "CHECK");
                foreach (var element in childNode)
                {
                    var checkBox = new UICheckBox();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) checkBox.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) checkBox.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) checkBox.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) checkBox.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) checkBox.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            checkBox.Hint = textNode.Attributes["String"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "NormalImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "NormalImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                checkBox.NormalImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        // Not used
                        //if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "CheckedImage"))
                        //{
                        //    var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "CheckedImage");
                        //    if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                        //        checkBox.CheckedImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        //}
                    }
                    Dialog.CheckBoxes.Add(checkBox);

                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "STILLIMAGEBUTTON");
                foreach (var element in childNode)
                {
                    var buttonControl = new UIStillImageButton();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) buttonControl.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) buttonControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) buttonControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) buttonControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) buttonControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            buttonControl.Hint = textNode.Attributes["String"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) buttonControl.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) buttonControl.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) buttonControl.FontName = textNode.Attributes["FontName"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) buttonControl.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) buttonControl.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "InnerColor")) buttonControl.InnerColor = textNode.Attributes["InnerColor"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameUpImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameUpImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                buttonControl.FrameUpImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        // Not used
                        //if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameDownImage"))
                        //{
                        //    var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameDownImage");
                        //    if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                        //        buttonControl.FrameDownImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        //}
                        //if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameOnHoverImage"))
                        //{
                        //    var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameOnHoverImage");
                        //    if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                        //        buttonControl.FrameOnHoverImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        //}
                    }
                    Dialog.StillImageButtons.Add(buttonControl);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "LIST");
                foreach (var element in childNode)
                {
                    var listBox = new UIListBox();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) listBox.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) listBox.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) listBox.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) listBox.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) listBox.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MultiLine")) listBox.MultiLine = element.Attributes["MultiLine"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "LineSpace")) listBox.LineSpace = Convert.ToInt32(element.Attributes["LineSpace"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            listBox.Hint = textNode.Attributes["String"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) listBox.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) listBox.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) listBox.FontName = textNode.Attributes["FontName"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) listBox.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) listBox.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "InnerTextColor")) listBox.InnerTextColor = textNode.Attributes["InnerTextColor"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                listBox.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "HilightImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "HilightImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                listBox.HilightImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "UpImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "UpImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                listBox.UpImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "DownImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "DownImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                listBox.DownImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "ScrollImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "ScrollImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                listBox.ScrollImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "BarImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "BarImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                listBox.BarImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.ListBoxes.Add(listBox);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "TREE");
                foreach (var element in childNode)
                {
                    var Tree = new UITree();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) Tree.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) Tree.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) Tree.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) Tree.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) Tree.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Indent")) Tree.Indent = Convert.ToInt32(element.Attributes["Indent"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            Tree.Hint = textNode.Attributes["String"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) Tree.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) Tree.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) Tree.FontName = textNode.Attributes["FontName"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) Tree.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) Tree.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "InnerTextColor")) Tree.InnerTextColor = textNode.Attributes["InnerTextColor"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "UpImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "UpImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.UpImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "DownImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "DownImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.DownImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "HilightImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "HilightImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.HilightImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "ScrollImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "ScrollImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.ScrollImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "BarImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "BarImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.BarImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "PlusSymbolImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "PlusSymbolImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.PlusSymbolImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "MinusSymbolImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "MinusSymbolImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.MinusSymbolImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "LeafSymbolImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "LeafSymbolImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                Tree.LeafSymbolImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.Trees.Add(Tree);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "TEXT");
                foreach (var element in childNode)
                {
                    var textArea = new UITextArea();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) textArea.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) textArea.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) textArea.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) textArea.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) textArea.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "LineSpace")) textArea.LineSpace = Convert.ToInt32(element.Attributes["LineSpace"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            textArea.Hint = textNode.Attributes["String"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) textArea.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) textArea.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) textArea.FontName = textNode.Attributes["FontName"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) textArea.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) textArea.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "InnerTextColor")) textArea.InnerTextColor = textNode.Attributes["InnerTextColor"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                textArea.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "UpImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "UpImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                textArea.UpImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "DownImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "DownImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                textArea.DownImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "ScrollImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "ScrollImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                textArea.ScrollImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "BarImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "BarImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                textArea.BarImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.TextAreas.Add(textArea);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "SLIDER");
                foreach (var element in childNode)
                {
                    var control = new UISlider();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) control.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) control.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) control.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) control.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) control.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            control.Hint = textNode.Attributes["String"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                control.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "BarImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "BarImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                control.BarImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.Sliders.Add(control);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "COMBO");
                foreach (var element in childNode)
                {
                    var comboBox = new UIComboBox();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) comboBox.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) comboBox.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) comboBox.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) comboBox.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) comboBox.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            comboBox.Hint = textNode.Attributes["String"].Value;
                    }
                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) comboBox.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) comboBox.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) comboBox.FontName = textNode.Attributes["FontName"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) comboBox.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) comboBox.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TextUpperColor")) comboBox.TextUpperColor = textNode.Attributes["TextUpperColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TextLowerColor")) comboBox.TextLowerColor = textNode.Attributes["TextLowerColor"].Value;
                    }

                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Resource"))
                    {
                        var resourceNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Resource");

                        if (resourceNode.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "FrameImage"))
                        {
                            var imagePathNode = resourceNode.ChildNodes.Cast<XmlNode>().First(t => t.Name == "FrameImage");
                            if (imagePathNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName"))
                                comboBox.FrameImage = string.Format(@"{0}\{1}", path, imagePathNode.Attributes["FileName"].Value);
                        }
                    }
                    Dialog.ComboBoxes.Add(comboBox);
                }
                childNode = xmlDocChilds.Cast<XmlNode>().Where(t => t.Name == "LABEL");
                foreach (var element in childNode)
                {
                    var labelControl = new UILabel();
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name")) labelControl.Name = element.Attributes["Name"].Value;
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "x")) labelControl.X = Convert.ToDouble(element.Attributes["x"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "y")) labelControl.Y = Convert.ToDouble(element.Attributes["y"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Width")) labelControl.Width = Convert.ToDouble(element.Attributes["Width"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height")) labelControl.Height = Convert.ToDouble(element.Attributes["Height"].Value);
                    if (element.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Align")) labelControl.Align = Convert.ToInt32(element.Attributes["Align"].Value);
                    if (element.ChildNodes.Cast<XmlNode>().Any(t => t.Name == "Hint"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Hint");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String"))
                            labelControl.Hint = textNode.Attributes["String"].Value;
                    }
                    if (element.ChildNodes.Cast<XmlNode>().Any(node => node.Name == "Text"))
                    {
                        var textNode = element.ChildNodes.Cast<XmlNode>().First(node => node.Name == "Text");
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "String")) labelControl.Text = textNode.Attributes["String"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontSize")) labelControl.FontSize = Convert.ToDouble(textNode.Attributes["FontSize"].Value);
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FontName")) labelControl.FontName = textNode.Attributes["FontName"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Color")) labelControl.Color = textNode.Attributes["Color"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "OutlineColor")) labelControl.OutlineColor = textNode.Attributes["OutlineColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TextUpperColor")) labelControl.TextUpperColor = textNode.Attributes["TextUpperColor"].Value;
                        if (textNode.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TextLowerColor")) labelControl.TextLowerColor = textNode.Attributes["TextLowerColor"].Value;
                    }
                    Dialog.Labels.Add(labelControl);
                }

                File.Delete(tmpFileName);
            }
            catch (Exception e)
            {
                return e;
            }

            return null;
        }

        public void RefreshLayout(Canvas dialogCanvas)
        {
            dialogCanvas.Children.Clear();
            dialogCanvas.Children.Add(new Image
            {
                ToolTip = Dialog.Name,
                Width = Dialog.Width,
                Height = Dialog.Height,
                Stretch = Stretch.Fill,
                StretchDirection = StretchDirection.Both,
                Source = Dialog.FrameImage == null ? new BitmapImage() : Core.TrueStretchImage(Dialog.FrameImage, Dialog.Width, Dialog.Height)
            });
            foreach (var control in Dialog.Edits)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage != null ? control.FrameImage : null, control.Width, control.Height)
                });
                var tb = new TextBlock
                {
                    ToolTip = control.Name,
                    Text = MainWindow.ElementsName == 1 ? control.Name : control.Text,
                    Width = control.Width == 0 ? Double.NaN : control.Width,
                    Height = control.Height == 0 ? Double.NaN : control.Height,
                    Margin = new Thickness(control.X, control.Y + ((control.Height - control.FontSize + 3) / 4), 0, 0),
                    FontFamily = string.IsNullOrEmpty(control.FontName) ? new FontFamily("方正细黑一简体") : new FontFamily(control.FontName),
                    Foreground = Core.GetColorBrushFromString(control.Color)
                };

                if (control.FontSize > 1) tb.FontSize = control.FontSize + 3;
                dialogCanvas.Children.Add(tb);
            }
            foreach (var pic in Dialog.ImagePictures)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = pic.Name,
                    Width = pic.Width,
                    Height = pic.Height,
                    Stretch = Stretch.Fill,
                    Margin = new Thickness(pic.X, pic.Y, 0, 0),
                    Source = pic.Frames > 1 ? Core.FrameImage(pic.FileName, pic.Width, pic.Height, pic.Frames) : Core.GetImageSourceFromFileName(pic.FileName)
                });

                if (MainWindow.ElementsName == 1)
                {
                    var tb = new TextBlock
                    {
                        ToolTip = pic.Name,
                        Text = pic.Name,
                        Margin = new Thickness(pic.X, pic.Y, 0, 0),
                        Foreground = Brushes.White
                    };
                    dialogCanvas.Children.Add(tb);
                }

            }
            foreach (var control in Dialog.Scrolls)
            {
                var scrollControl = new Canvas
                {
                    ToolTip = control.Name,
                    Width = control.Width,
                    Height = control.Height,
                    Margin = new Thickness(control.X, control.Y, 0, 0)
                };
                if (!string.IsNullOrEmpty(control.ScrollImage))
                {
                    var scrollImage = new Image { Source = Core.GetImageSourceFromFileName(control.ScrollImage), Height = control.Height, Stretch = Stretch.Fill };
                    scrollImage.Width = scrollImage.Source.Width;
                    scrollControl.Children.Add(scrollImage);
                    double scrollCanvasWidth = scrollImage.Source.Width;
                    scrollControl.Margin = new Thickness(control.X + control.Width - scrollCanvasWidth, control.Y, 0, 0);
                }
                if (!string.IsNullOrEmpty(control.BarFrameImage))
                {
                    var barImage = new Image { Source = Core.GetImageSourceFromFileName(control.BarFrameImage), Margin = new Thickness(0, control.Height / 2, 0, 0) };
                    scrollControl.Children.Add(barImage);
                }
                if (!string.IsNullOrEmpty(control.UpImage))
                {
                    var upImageSource = Core.GetImageSourceFromFileName(control.UpImage);
                    var upImage = new Image { Source = Core.FrameImage(control.UpImage, upImageSource.Width, upImageSource.Height / 2, 2) };
                    scrollControl.Children.Add(upImage);
                }
                if (!string.IsNullOrEmpty(control.DownImage))
                {
                    var downImageSource = Core.GetImageSourceFromFileName(control.DownImage);
                    var downImage = new Image { Source = Core.FrameImage(control.DownImage, downImageSource.Width, downImageSource.Height / 2, 2), Margin = new Thickness(0, control.Height - (downImageSource.Height / 2), 0, 0) };
                    scrollControl.Children.Add(downImage);
                }
                dialogCanvas.Children.Add(scrollControl);
            }
            foreach (var control in Dialog.RadioButtons)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.GetImageSourceFromFileName(control.NormalImage)
                });
                if (MainWindow.ElementsName == 1)
                {
                    var tb = new TextBlock
                    {
                        ToolTip = control.Name,
                        Text = control.Name,
                        Margin = new Thickness(control.X, control.Y, 0, 0),
                        Foreground = Brushes.White
                    };
                    dialogCanvas.Children.Add(tb);
                }
            }
            foreach (var control in Dialog.CheckBoxes)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.GetImageSourceFromFileName(control.NormalImage)
                });
                if (MainWindow.ElementsName == 1)
                {
                    var tb = new TextBlock
                    {
                        ToolTip = control.Name,
                        Text = control.Name,
                        Margin = new Thickness(control.X, control.Y, 0, 0),
                        Foreground = Brushes.White
                    };
                    dialogCanvas.Children.Add(tb);
                }
            }
            foreach (var control in Dialog.StillImageButtons)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameUpImage, control.Width, control.Height)
                });
                var tb = new TextBlock
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Text = MainWindow.ElementsName == 1 ? control.Name : control.Text,
                    Width = control.Width,
                    Height = control.Height,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(control.X, control.Y + ((control.Height - control.FontSize + 3) / 4), 0, 0),
                    Foreground = string.IsNullOrEmpty(control.Color) ? Brushes.White : Core.GetColorBrushFromString(control.Color),
                    FontFamily = string.IsNullOrEmpty(control.FontName) ? new FontFamily("方正细黑一简体") : new FontFamily(control.FontName),
                };
                if (control.FontSize > 1) tb.FontSize = control.FontSize + 3;
                dialogCanvas.Children.Add(tb);
            }
            foreach (var control in Dialog.ProgressBars)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage, control.Width, control.Height)
                });
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.GetImageSourceFromFileName(control.FillImage)
                });

                if (MainWindow.ElementsName == 1)
                {
                    var tb = new TextBlock
                    {
                        ToolTip = control.Name,
                        Text = control.Name,
                        Margin = new Thickness(control.X, control.Y, 0, 0),
                        Foreground = Brushes.White
                    };
                    dialogCanvas.Children.Add(tb);
                }
            }
            foreach (var control in Dialog.Sliders)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage, control.Width, control.Height)
                });

                Image barImage = new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Margin = new Thickness(control.X + control.Width / 3, control.Y, 0, 0),
                    Source = Core.GetImageSourceFromFileName(control.BarImage)
                };
                dialogCanvas.Children.Add(barImage);

                if (MainWindow.ElementsName == 1)
                {
                    var tb = new TextBlock
                    {
                        ToolTip = control.Name,
                        Text = control.Name,
                        Margin = new Thickness(control.X, control.Y, 0, 0),
                        Foreground = Brushes.White
                    };
                    dialogCanvas.Children.Add(tb);
                }
            }
            foreach (var control in Dialog.Labels)
            {
                var tb = new TextBlock
                {
                    Text = string.IsNullOrEmpty(control.Text) && MainWindow.ElementsName == 1 ? control.Name : control.Text,
                    Width = control.Width == 0 ? Double.NaN : control.Width,
                    Height = control.Height == 0 ? Double.NaN : control.Height,
                    TextWrapping = TextWrapping.WrapWithOverflow,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Foreground = string.IsNullOrEmpty(control.Color) ? Brushes.White : Core.GetColorBrushFromString(control.Color),
                    FontFamily = string.IsNullOrEmpty(control.FontName) ? new FontFamily("方正细黑一简体") : new FontFamily(control.FontName),
                };
                if (control.Align == 1) tb.TextAlignment = TextAlignment.Center;
                if (control.FontSize > 1) tb.FontSize = control.FontSize + 3;
                dialogCanvas.Children.Add(tb);
            }
            foreach (var control in Dialog.ListBoxes)
            {
                //Background
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage != null ? control.FrameImage : null, control.Width, control.Height)
                });

                //Scroll
                double scrollCanvasWidth = 0;
                var scrollControl = new Canvas
                {
                    ToolTip = control.Name + ": Scroll",
                    Margin = new Thickness(control.X + control.Width, control.Y, 0, 0)
                };

                if (!string.IsNullOrEmpty(control.ScrollImage))
                {
                    var scrollImage = new Image { Source = Core.GetImageSourceFromFileName(control.ScrollImage), Height = control.Height, Stretch = Stretch.Fill };
                    scrollImage.Width = scrollImage.Source.Width;
                    scrollControl.Children.Add(scrollImage);
                    scrollCanvasWidth = scrollImage.Source.Width;
                    scrollControl.Margin = new Thickness(control.X + control.Width - scrollCanvasWidth, control.Y, 0, 0);
                }
                if (!string.IsNullOrEmpty(control.BarImage))
                {
                    var barImage = new Image { Source = Core.GetImageSourceFromFileName(control.BarImage), Margin = new Thickness(0, control.Height / 2, 0, 0) };
                    scrollControl.Children.Add(barImage);
                }
                if (!string.IsNullOrEmpty(control.UpImage))
                {
                    var upImageSource = Core.GetImageSourceFromFileName(control.UpImage);
                    var upImage = new Image { Source = Core.FrameImage(control.UpImage, upImageSource.Width, upImageSource.Height / 2, 2) };
                    scrollControl.Children.Add(upImage);
                }
                if (!string.IsNullOrEmpty(control.DownImage))
                {
                    var downImageSource = Core.GetImageSourceFromFileName(control.DownImage);
                    var downImage = new Image { Source = Core.FrameImage(control.DownImage, downImageSource.Width, downImageSource.Height / 2, 2), Margin = new Thickness(0, control.Height - (downImageSource.Height / 2), 0, 0) };
                    scrollControl.Children.Add(downImage);
                }
                dialogCanvas.Children.Add(scrollControl);

                //Line hilight
                ImageBrush hilightBrush = new ImageBrush();
                if (!string.IsNullOrEmpty(control.HilightImage))
                {
                    hilightBrush.ImageSource = Core.GetImageSourceFromFileName(control.HilightImage);
                }
                Canvas canvasHilightLine = new Canvas
                {
                    Height = control.LineSpace + control.FontSize + 5,
                    Width = control.Width - scrollCanvasWidth,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Background = hilightBrush
                };
                dialogCanvas.Children.Add(canvasHilightLine);

                //Colums margin
                Regex regex = new Regex(@"\d+");
                if (!string.IsNullOrEmpty(control.MultiLine))
                {
                    MatchCollection matchCollection = regex.Matches(control.MultiLine);
                    foreach (Match match in matchCollection)
                    {
                        control.MultiLineValues.Add(Convert.ToInt32(match.Value));
                    }
                }
                else
                {
                    control.MultiLineValues.Add(0);
                }

                //Lines
                int lbLineCount = (int)Math.Floor(control.Height / (control.FontSize + 5 + control.LineSpace));
                int columnMargin = 0;
                for (var k = 0; k < control.MultiLineValues.Count; k++)
                {
                    for (var i = 0; i < lbLineCount; i++)
                    {
                        TextBlock tb = new TextBlock
                        {
                            ToolTip = control.Name,
                            Text = "Line " + i,
                            Margin = new Thickness(control.X + columnMargin, control.Y + control.LineSpace * i + i * (control.FontSize + 5), 0, 0),
                            FontFamily = string.IsNullOrEmpty(control.FontName) ? new FontFamily("方正细黑一简体") : new FontFamily(control.FontName),
                            Foreground = string.IsNullOrEmpty(control.Color) ? Brushes.White : Core.GetColorBrushFromString(control.Color),
                            Padding = new Thickness(0, control.LineSpace / 2, 0, 0),
                            FontSize = control.FontSize + 3
                        };
                        dialogCanvas.Children.Add(tb);
                    }
                    columnMargin = columnMargin + control.MultiLineValues[k];
                }
            }
            foreach (var control in Dialog.TextAreas)
            {
                //Background
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage != null ? control.FrameImage : null, control.Width, control.Height)
                });


                //Scroll
                double scrollCanvasWidth = 0;
                var scrollControl = new Canvas
                {
                    ToolTip = control.Name + ": Scroll",
                    Height = control.Height,
                    Margin = new Thickness(control.X + control.Width, control.Y, 0, 0)
                };

                if (!string.IsNullOrEmpty(control.ScrollImage))
                {
                    var scrollImage = new Image { Source = Core.GetImageSourceFromFileName(control.ScrollImage), Height = control.Height, Stretch = Stretch.Fill };
                    scrollControl.Children.Add(scrollImage);
                    scrollImage.Width = scrollImage.Source.Width;
                    scrollCanvasWidth = scrollImage.Source.Width;
                    scrollControl.Margin = new Thickness(control.X + control.Width - scrollCanvasWidth, control.Y, 0, 0);
                }
                if (!string.IsNullOrEmpty(control.BarImage))
                {
                    var barImage = new Image { Source = Core.GetImageSourceFromFileName(control.BarImage), Margin = new Thickness(0, control.Height / 2, 0, 0) };
                    scrollControl.Children.Add(barImage);
                }
                if (!string.IsNullOrEmpty(control.UpImage))
                {
                    var upImageSource = Core.GetImageSourceFromFileName(control.UpImage);
                    var upImage = new Image { Source = Core.FrameImage(control.UpImage, upImageSource.Width, upImageSource.Height / 2, 2) };
                    scrollControl.Children.Add(upImage);
                }
                if (!string.IsNullOrEmpty(control.DownImage))
                {
                    var downImageSource = Core.GetImageSourceFromFileName(control.DownImage);
                    var downImage = new Image { Source = Core.FrameImage(control.DownImage, downImageSource.Width, downImageSource.Height / 2, 2), Margin = new Thickness(0, control.Height - (downImageSource.Height / 2), 0, 0) };
                    scrollControl.Children.Add(downImage);
                }
                dialogCanvas.Children.Add(scrollControl);

                //Line hilight
                ImageBrush hilightBrush = new ImageBrush();
                if (!string.IsNullOrEmpty(control.HilightImage))
                {
                    hilightBrush.ImageSource = Core.GetImageSourceFromFileName(control.HilightImage);
                }
                Canvas canvasHilightLine = new Canvas
                {
                    Height = control.LineSpace + control.FontSize + 5,
                    Width = control.Width - scrollCanvasWidth,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Background = hilightBrush
                };
                dialogCanvas.Children.Add(canvasHilightLine);

                //Text
                TextBlock tb = new TextBlock
                {
                    Text = MainWindow.ElementsName==1?control.Name:control.Text,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Height = control.Height,
                    Width = control.Width,
                    LineHeight = control.LineSpace > 0 ? control.LineSpace + control.FontSize + 3 : Double.NaN,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = string.IsNullOrEmpty(control.FontName) ? new FontFamily("方正细黑一简体") : new FontFamily(control.FontName),
                    Foreground = Core.GetColorBrushFromString(control.Color),
                    FontSize = control.FontSize + 3
                };
                dialogCanvas.Children.Add(tb);
            }
            foreach (var control in Dialog.ComboBoxes)
            {
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage, control.Width, control.Height)
                });
                var tb = new TextBlock
                {
                    Text = MainWindow.ElementsName == 1 ? control.Name : control.Text,
                    Width = control.Width,
                    Height = control.Height,
                    Margin = new Thickness(control.X + control.Width / 4, control.Y + ((control.Height - control.FontSize + 3) / 4), 0, 0),
                    Foreground = string.IsNullOrEmpty(control.Color) ? Brushes.White : Core.GetColorBrushFromString(control.Color),
                    FontFamily = string.IsNullOrEmpty(control.FontName) ? new FontFamily("方正细黑一简体") : new FontFamily(control.FontName),
                };
                if (control.FontSize > 1) tb.FontSize = control.FontSize + 3;
                dialogCanvas.Children.Add(tb);
            }
            foreach (var control in Dialog.Trees)
            {
                //Background
                dialogCanvas.Children.Add(new Image
                {
                    ToolTip = string.IsNullOrEmpty(control.Hint) ? control.Name : string.Format("{0}: {1}", control.Name, control.Hint),
                    Width = control.Width,
                    Height = control.Height,
                    Stretch = Stretch.Fill,
                    StretchDirection = StretchDirection.Both,
                    Margin = new Thickness(control.X, control.Y, 0, 0),
                    Source = Core.TrueStretchImage(control.FrameImage != null ? control.FrameImage : null, control.Width, control.Height)
                });

                //Scroll
                var scrollControl = new Canvas
                {
                    ToolTip = control.Name + ": Scroll",
                    Margin = new Thickness(control.X + control.Width, control.Y, 0, 0)
                };

                if (!string.IsNullOrEmpty(control.ScrollImage))
                {
                    var scrollImage = new Image { Source = Core.GetImageSourceFromFileName(control.ScrollImage), Height = control.Height, Stretch = Stretch.Fill };
                    scrollImage.Width = scrollImage.Source.Width;
                    scrollControl.Children.Add(scrollImage);
                    double scrollCanvasWidth = scrollImage.Source.Width;
                    scrollControl.Margin = new Thickness(control.X + control.Width - scrollCanvasWidth, control.Y, 0, 0);
                }
                if (!string.IsNullOrEmpty(control.BarImage))
                {
                    var barImage = new Image { Source = Core.GetImageSourceFromFileName(control.BarImage), Margin = new Thickness(0, control.Height / 2, 0, 0) };
                    scrollControl.Children.Add(barImage);
                }
                if (!string.IsNullOrEmpty(control.UpImage))
                {
                    var upImageSource = Core.GetImageSourceFromFileName(control.UpImage);
                    var upImage = new Image { Source = Core.FrameImage(control.UpImage, upImageSource.Width, upImageSource.Height / 2, 2) };
                    scrollControl.Children.Add(upImage);
                }
                if (!string.IsNullOrEmpty(control.DownImage))
                {
                    var downImageSource = Core.GetImageSourceFromFileName(control.DownImage);
                    var downImage = new Image { Source = Core.FrameImage(control.DownImage, downImageSource.Width, downImageSource.Height / 2, 2), Margin = new Thickness(0, control.Height - (downImageSource.Height / 2), 0, 0) };
                    scrollControl.Children.Add(downImage);
                }
                dialogCanvas.Children.Add(scrollControl);

                //Hilight brush
                ImageBrush hilightBrush = new ImageBrush();
                if (!string.IsNullOrEmpty(control.HilightImage))
                {
                    hilightBrush.ImageSource = Core.GetImageSourceFromFileName(control.HilightImage);
                }


                var treeStack = new StackPanel();
                treeStack.Orientation = Orientation.Vertical;
                treeStack.Margin = new Thickness(control.X, control.Y, 0, 0);

                var treeLineStack = new StackPanel();
                treeLineStack.Orientation = Orientation.Horizontal;
                var plusImage = new Image
                {
                    Margin = new Thickness(0, control.Indent, 0, control.Indent),
                    Source = Core.GetImageSourceFromFileName(control.PlusSymbolImage)
                };
                treeLineStack.Children.Add(plusImage);
                treeLineStack.Children.Add(new TextBlock
                {
                    Text = "Line 1",
                    FontSize = control.FontSize + 3,
                    Foreground = Brushes.White,
                    Background = hilightBrush,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3, 0, 0, 0)
                });

                var treeLineStack2 = new StackPanel();
                treeLineStack2.Orientation = Orientation.Horizontal;
                double leafMargin = 0;
                if (!string.IsNullOrEmpty(control.MinusSymbolImage))
                {
                    var minusImage = new Image
                    {
                        Margin = new Thickness(0, control.Indent, 0, control.Indent),
                        Source = Core.GetImageSourceFromFileName(control.MinusSymbolImage)
                    };
                    leafMargin = minusImage.Source.Width;
                    treeLineStack2.Children.Add(minusImage);
                }
                treeLineStack2.Children.Add(new TextBlock
                {
                    Text = "Line 2",
                    FontSize = control.FontSize + 3,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3, 0, 0, 0)
                });

                var treeLineStack3 = new StackPanel();
                treeLineStack3.Orientation = Orientation.Horizontal;
                treeLineStack3.Children.Add(new Image
                {
                    Margin = new Thickness(leafMargin, control.Indent, 0, control.Indent),
                    Source = Core.GetImageSourceFromFileName(control.LeafSymbolImage)
                });
                treeLineStack3.Children.Add(new TextBlock
                {
                    Text = "Line 3",
                    FontSize = control.FontSize + 3,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(3, 0, 0, 0)
                });

                treeStack.Children.Add(treeLineStack);
                treeStack.Children.Add(treeLineStack2);
                treeStack.Children.Add(treeLineStack3);
                dialogCanvas.Children.Add(treeStack);
            }
        }
    }
}
