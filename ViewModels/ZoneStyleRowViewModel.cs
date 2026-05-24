using ELVZone.Models;

namespace ELVZone.ViewModels
{
    public class ZoneStyleRowViewModel
    {
        public string Title { get; }
        public ZoneStyleSetting Style { get; }

        public ZoneStyleRowViewModel(string title, ZoneStyleSetting style)
        {
            Title = title;
            Style = style;
        }
    }
}
