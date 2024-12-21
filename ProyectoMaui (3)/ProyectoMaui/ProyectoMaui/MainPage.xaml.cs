namespace ProyectoMaui
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnOrderButtonClicked(object sender, EventArgs e)
        {
            // Aquí puedes navegar a la página de Login o la página de Menú
            Navigation.PushAsync(new LoginPage());
        }

    }

}