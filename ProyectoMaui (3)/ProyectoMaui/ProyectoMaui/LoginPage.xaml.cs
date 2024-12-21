using ProyectoMaui;
using ProyectoMaui.DTO;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Maui.Storage;

namespace ProyectoMaui;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
    }

    // Método para iniciar sesión
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        // Forzar el cierre del teclado
        CerrarTeclado();

        if (this.validarDatos())
        {
            try
            {
                // Mostrar el indicador de carga
                LoadingGrid.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingLabel.Text = "Esperando...";

                ReqLogin req = new ReqLogin
                {
                    Login = new Login
                    {
                        email = txtEmail.Text,
                        password = txtPassword.Text
                    }
                };

                var jsonContent = new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/login/entrarlogin", jsonContent);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var res = JsonConvert.DeserializeObject<ResLogin>(responseContent);

                        if (res.resultado)
                        {
                            await GuardarSesion(res.sesion_id, res.id_usuario, res.email, res.nombre);

                            LoadingLabel.Text = "Inicio de sesión exitoso";
                            await Task.Delay(2000);

                            await Navigation.PushAsync(new MenuPage());
                        }
                        else
                        {
                            LoadingGrid.IsVisible = false;
                            LoadingIndicator.IsRunning = false;

                            await DisplayAlert("Error de inicio de sesión", res.listaDeErrores.First().error, "Aceptar");
                        }
                    }
                    else
                    {
                        LoadingGrid.IsVisible = false;
                        LoadingIndicator.IsRunning = false;

                        await DisplayAlert("Error de conexión", "No fue posible conectar con el servidor", "Aceptar");
                    }
                }
            }
            catch (Exception ex)
            {
                LoadingGrid.IsVisible = false;
                LoadingIndicator.IsRunning = false;

                await DisplayAlert("Error inesperado", $"Ocurrió un error: {ex.Message}", "Aceptar");
            }
        }
        else
        {
            await DisplayAlert("Datos faltantes", "Por favor, complete todos los datos", "Aceptar");
        }
    }

    // Método para cerrar el teclado
    private void CerrarTeclado()
    {
#if ANDROID
        if (txtEmail.Handler.PlatformView is Android.Views.View view)
        {
            var inputMethodManager = Android.App.Application.Context.GetSystemService(Android.Content.Context.InputMethodService) as Android.Views.InputMethods.InputMethodManager;
            inputMethodManager?.HideSoftInputFromWindow(view.WindowToken, Android.Views.InputMethods.HideSoftInputFlags.None);
        }
#endif
        // Implementaciones para otras plataformas (si es necesario)
    }

    private bool validarDatos()
    {
        return !string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(txtPassword.Text);
    }

    private async Task GuardarSesion(long? sesionId, int? idUsuario, string email, string nombre)
    {
        try
        {
            if (sesionId.HasValue && idUsuario.HasValue)
            {
                await SecureStorage.SetAsync("sesion_id", sesionId.Value.ToString());
                await SecureStorage.SetAsync("id_usuario", idUsuario.Value.ToString());
                await SecureStorage.SetAsync("email", email);
                await SecureStorage.SetAsync("nombre", nombre);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error al guardar sesión", $"Ocurrió un error al guardar los datos: {ex.Message}", "Aceptar");
        }
    }

    private async void OnRegisterTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterarsePage());
    }

    public static async Task LimpiarSesion()
    {
        SecureStorage.Remove("sesion_id");
        SecureStorage.Remove("id_usuario");
        SecureStorage.Remove("email");
        SecureStorage.Remove("nombre");
    }

    public static async Task<(long?, int?, string, string)> CargarSesion()
    {
        try
        {
            var sesionIdStr = await SecureStorage.GetAsync("sesion_id");
            var idUsuarioStr = await SecureStorage.GetAsync("id_usuario");
            var email = await SecureStorage.GetAsync("email");
            var nombre = await SecureStorage.GetAsync("nombre");

            long? sesionId = string.IsNullOrEmpty(sesionIdStr) ? null : long.Parse(sesionIdStr);
            int? idUsuario = string.IsNullOrEmpty(idUsuarioStr) ? null : int.Parse(idUsuarioStr);

            return (sesionId, idUsuario, email, nombre);
        }
        catch
        {
            return (null, null, null, null);
        }
    }
}
