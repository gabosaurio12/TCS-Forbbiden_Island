using System.Windows.Media;

namespace Forbbiden.Client.Model
{
    public class CardWindowSettings
    {
        public int StrokeThickness { get; set; }
        public string StrokeColor { get; set; }
        public ImageSource CardImage { get; set; }

        public CardWindowSettings()
        {
        }
    }
}
