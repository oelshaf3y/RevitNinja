using System.Windows;
using Revit_Ninja.Commands.BIMSubmittal;

namespace Revit_Ninja.Views.BIMSubmittal
{
    /// <summary>
    /// Interaction logic for SubmitFedModelView.xaml
    /// </summary>
    public partial class SubmitFedModelView : Window
    {
        public List<linkforset> RLIS = new List<linkforset>();
        public SubmitFedModelView(List<linkforset> rlis)
        {
            InitializeComponent();
            RLIS = rlis;
            foreach (linkforset rli in RLIS)
            {
                LinkedModelForSubmit UC = new LinkedModelForSubmit(rli);
                stack.Children.Add(UC);
            }
        }


        private void ok_click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < RLIS.Count; i++)
            {
                LinkedModelForSubmit uc = (LinkedModelForSubmit)stack.Children[i];
                RLIS[i] = uc.updateLink();
            }
            DialogResult = true;
        }

        private void cancel_click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void stack_Scroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {

        }
    }
}
