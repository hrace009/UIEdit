namespace UIEdit.Models
{
    public class UITree : UIListBox
    {
        public string PlusSymbolImage { get; set; }
        public string MinusSymbolImage { get; set; }
        public string LeafSymbolImage { get; set; }
        public int Indent { get; set; }
    }
}
