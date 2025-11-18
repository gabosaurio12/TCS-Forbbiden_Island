using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Forbbiden.Client.model
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
