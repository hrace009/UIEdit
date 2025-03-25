using System.Collections.Generic;

namespace UIEdit.Models
{
    public class UIDialog
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string FrameImage { get; set; }
        public List<UIImagePicture> ImagePictures { get; set; }
        public List<UIScroll> Scrolls { get; set; }
        public List<UIEditBox> Edits { get; set; }
        public List<UIStillImageButton> StillImageButtons { get; set; }
        public List<UIRadioButton> RadioButtons { get; set; }
        public List<UICheckBox> CheckBoxes { get; set; }
        public List<UILabel> Labels { get; set; }
        public List<UIListBox> ListBoxes { get; set; }
        public List<UITextArea> TextAreas { get; set; }
        public List<UIComboBox> ComboBoxes { get; set; }
        public List<UIProgressBar> ProgressBars { get; set; }
        public List<UISlider> Sliders { get; set; }
        public List<UITree> Trees { get; set; }

        public UIDialog()
        {
            ImagePictures = new List<UIImagePicture>();
            Scrolls = new List<UIScroll>();
            Edits = new List<UIEditBox>();
            StillImageButtons = new List<UIStillImageButton>();
            RadioButtons = new List<UIRadioButton>();
            CheckBoxes = new List<UICheckBox>();
            Labels = new List<UILabel>();
            ListBoxes = new List<UIListBox>();
            TextAreas = new List<UITextArea>();
            ComboBoxes = new List<UIComboBox>();
            ProgressBars = new List<UIProgressBar>();
            Sliders = new List<UISlider>();
            Trees = new List<UITree>();
        }
    }
}
