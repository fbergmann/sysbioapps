namespace Welcome.Models
{
    public class CurrentTranslation
    {
        public string FileName { get; set; }
        public string Translation { get; set; }
        public string CurrentTranslator { get; set; }
        public Translator[] Translators { get; set; }        
    }
}