
using System.Windows.Controls;
using TheScheduler.Models;

namespace TheScheduler.Components
{
    /// <summary>
    /// EmployeeInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class EmployeeInfo : UserControl
    {
        public Array SexValues { get; } = Enum.GetValues(typeof(Sex));
        public EmployeeInfo()
        {
            InitializeComponent();
        }
    }
}
