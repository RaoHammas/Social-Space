using System.Windows.Controls;

namespace SocialSpace
{
    /// <summary>
    /// Interaction logic for ControlSocialPage.xaml
    /// </summary>
    public partial class ControlSocialPage : UserControl
    {
        public ControlSocialPage()
        {
            InitializeComponent();
        }

        public SocialApp App { get; set; }
        public SocialPage Page { get; set; }


    }
}
