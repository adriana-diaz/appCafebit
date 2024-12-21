using ProyectoMaui;
using ProyectoMaui.DTO;
using System.Text;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace ProyectoMaui;

public partial class RegisterarsePage : ContentPage
{
    public RegisterarsePage()
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        // Cerrar el teclado
        CerrarTeclado();

        // El usuario dio click al bot�n
        if (this.validarDatos())
        {
            try
            {
                // Mostrar el indicador de carga
                LoadingGrid.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingLabel.Text = "Esperando...";

                ReqIngresarUsuario req = new ReqIngresarUsuario();
                req.Usuario = new Usuario
                {
                    cedula = txtCedula.Text,
                    nombre = txtNombre.Text,
                    email = txtEmail.Text,
                    password = txtPassword.Text
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient()) // Creo la conexi�n HTTP
                {
                    var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/usuario/ingresarusuario", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ResIngresarUsuario>(responseContent);

                        if (res.resultado)
                        {
                            // Todo sali� bien
                            LoadingLabel.Text = "Registro exitoso";
                            await Task.Delay(2000); // Esperar 2 segundos para mostrar el mensaje

                            // Redirigir a la p�gina de inicio de sesi�n
                            await Navigation.PushAsync(new LoginPage());
                        }
                        else
                        {
                            // Error desde el backend
                            LoadingGrid.IsVisible = false;
                            LoadingIndicator.IsRunning = false;

                            await DisplayAlert("Error del backend", res.listaDeErrores.First().error, "Aceptar");
                        }
                    }
                    else
                    {
                        // No conect� con el API (No respondi�)
                        LoadingGrid.IsVisible = false;
                        LoadingIndicator.IsRunning = false;

                        await DisplayAlert("Error de conexi�n", "No fue posible conectar con el servidor", "Aceptar");
                    }
                } // Muere la conexi�n
            }
            catch (Exception ex)
            {
                // Ocultar el indicador de carga
                LoadingGrid.IsVisible = false;
                LoadingIndicator.IsRunning = false;

                await DisplayAlert("Error inesperado", $"Ocurri� un error no controlado: {ex.Message}", "Aceptar");
            }
        }
        else
        {
            await DisplayAlert("Datos faltantes", "Por favor, complete todos los datos", "Aceptar");
        }
    }

    private bool validarDatos()
    {
        return !string.IsNullOrEmpty(txtCedula.Text) &&
               !string.IsNullOrEmpty(txtNombre.Text) &&
               !string.IsNullOrEmpty(txtEmail.Text) &&
               !string.IsNullOrEmpty(txtPassword.Text);
    }

    private void CerrarTeclado()
    {
#if ANDROID
        if (txtCedula.Handler.PlatformView is Android.Views.View view)
        {
            var inputMethodManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.InputMethodService) as Android.Views.InputMethods.InputMethodManager;
            inputMethodManager?.HideSoftInputFromWindow(view.WindowToken, Android.Views.InputMethods.HideSoftInputFlags.None);
        }
#endif
    }
}
