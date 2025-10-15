using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheScheduler.Models;
using TheScheduler.ViewModels;

namespace TheScheduler.Components
{
    /// <summary>
    /// ScheduleEditDialog.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ScheduleEditSelector : UserControl
    {
        public ScheduleEditSelector()
        {
            InitializeComponent();
            this.PreviewKeyDown += UserControl_KeyDown;
            this.IsVisibleChanged += ScheduleEditSelector_IsVisibleChanged; // Hook up the new event handler
            DataContextChanged += ScheduleEditSelector_DataContextChanged;
        }

        private HomeViewModel VM => DataContext as HomeViewModel;

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                VM.IsVisibleScheduleEditDitalog = false;
            }
        }

        private void ScheduleEditSelector_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateDynamicContent();
        }

        private void UpdateDynamicContent()
        {
            if (VM == null) return;

            UserControl control = VM.Type switch
            {
                ScheduleEditType.Shift => new ScheduleDialog(),
                ScheduleEditType.Leave => new LeaveDialog(),
                ScheduleEditType.Correction => new CorrectionDialog()
            };

            if (control != null)
            {
                // 부모 ViewModel 그대로 전달
                control.DataContext = VM;
                DialogContent.Content = control;
            }
        }

        // 버튼 클릭 커맨드에서 CurrentType 변경 시 호출
        public void OnTypeChanged()
        {
            UpdateDynamicContent();
        }
        private void ScheduleEditSelector_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM != null && VM.TypeChanged == null)
                VM.TypeChanged = OnTypeChanged;
        }

        private void ScheduleEditSelector_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Keyboard.Focus(this);
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
        }
    }
}
