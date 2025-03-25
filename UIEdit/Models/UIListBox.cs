using System.Collections.Generic;

namespace UIEdit.Models
{
    public class UIListBox : UILabel {
        public string FrameImage { get; set; }
        public string HilightImage { get; set; }
        public string UpImage { get; set; }
        public string DownImage { get; set; }
        public string ScrollImage { get; set; }
        public string BarImage { get; set; }
        public string MultiLine { get; set; }
        public List<int> MultiLineValues { get; set; }
        public int LineSpace { get; set; } = 0;

        public UIListBox()
        {
            MultiLineValues = new List<int>();
        }
    }
}
