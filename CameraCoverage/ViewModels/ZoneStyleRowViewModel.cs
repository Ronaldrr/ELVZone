using ELVZone.Models;

namespace ELVZone.ViewModels
{
    public class ZoneStyleRowViewModel : ViewModelBase
    {
        public string Title { get; }
        public ZoneStyleSetting Style { get; }

        public ZoneStyleRowViewModel(string title, ZoneStyleSetting style)
        {
            Title = title;
            Style = style;
        }

        public bool FillEnabled
        {
            get => Style.FillEnabled;
            set
            {
                if (Style.FillEnabled == value)
                {
                    return;
                }

                Style.FillEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool LineEnabled
        {
            get => Style.LineEnabled;
            set
            {
                if (Style.LineEnabled == value)
                {
                    return;
                }

                Style.LineEnabled = value;
                OnPropertyChanged();
            }
        }

        public string FilledRegionTypeName
        {
            get => Style.FilledRegionTypeName;
            set
            {
                if (Style.FilledRegionTypeName == value)
                {
                    return;
                }

                Style.FilledRegionTypeName = value ?? string.Empty;
                OnPropertyChanged();
            }
        }

        public string LineStyleName
        {
            get => Style.LineStyleName;
            set
            {
                if (Style.LineStyleName == value)
                {
                    return;
                }

                Style.LineStyleName = value ?? string.Empty;
                OnPropertyChanged();
            }
        }
    }
}
