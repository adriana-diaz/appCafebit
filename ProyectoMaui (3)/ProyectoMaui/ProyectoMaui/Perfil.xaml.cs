using Newtonsoft.Json;
using ProyectoMaui.DTO;
using System.Text;

namespace ProyectoMaui;

public partial class Perfil : ContentPage
{
    private int _idUsuario;

    public Perfil()
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
        CargarDatosUsuario();
    }

    private async void CargarDatosUsuario()
    {
        try
        {
            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            var emailUsuario = await SecureStorage.GetAsync("email");
            var nombreUsuario = await SecureStorage.GetAsync("nombre");

            if (string.IsNullOrEmpty(idUsuarioString) || string.IsNullOrEmpty(emailUsuario) || string.IsNullOrEmpty(nombreUsuario))
            {
                await DisplayAlert("Error", "No se encontraron datos de usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                await Navigation.PopAsync();
                return;
            }

            _idUsuario = int.Parse(idUsuarioString);

            // Mostrar datos actuales en los campos
            NombreEntry.Text = nombreUsuario;
            EmailEntry.Text = emailUsuario;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar los datos del usuario: {ex.Message}", "Aceptar");
        }
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(NombreEntry.Text) || string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert("Error", "Por favor, complete los campos de Nombre y Email.", "Aceptar");
                return;
            }

            var usuarioActualizar = new ReqActualizarUsuario
            {
                usuario = new Actualizar
                {
                    id_usuario = _idUsuario,
                    nombre = NombreEntry.Text,
                    email = EmailEntry.Text,
                    password = string.IsNullOrWhiteSpace(PasswordEntry.Text) ? null : PasswordEntry.Text
                }
            };

            using (HttpClient client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(usuarioActualizar), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/usuario/actualizarusuario", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var contenido = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ResActualizarUsuario>(contenido);

                    if (resultado != null && resultado.Resultado)
                    {
                        // Actualizar datos en SecureStorage
                        await SecureStorage.SetAsync("nombre", NombreEntry.Text);
                        await SecureStorage.SetAsync("email", EmailEntry.Text);

                        await DisplayAlert("Éxito", "Tus datos han sido actualizados correctamente.", "Aceptar");
                        await Navigation.PushAsync(new MenuPage());
                    }
                    else
                    {
                        await DisplayAlert("Error", resultado?.Error ?? "Error desconocido al actualizar usuario.", "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo conectar al servidor.", "Aceptar");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al actualizar los datos: {ex.Message}", "Aceptar");
        }
    }
}
