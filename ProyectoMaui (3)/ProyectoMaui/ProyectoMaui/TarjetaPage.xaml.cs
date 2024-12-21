using Newtonsoft.Json;
using ProyectoMaui.DTO;
using System.Text;

namespace ProyectoMaui;

public partial class TarjetaPage : ContentPage
{
    private consultaTarjeta tarjetaSeleccionada;
    private string _total;

    public TarjetaPage(string total)
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
        _total = total; // Asignar el total recibido
        MostrarTotal();
        CargarTarjetasGuardadas();
    }

    private void MostrarTotal()
    {
        // Encuentra el label correspondiente y asigna el total
        var totalLabel = this.FindByName<Label>("TotalLabel");
        if (totalLabel != null)
        {
            totalLabel.Text = _total;
        }
    }

    private async void OnSavedCardsToggled(object sender, ToggledEventArgs e)
    {
        SavedCardsMenu.IsVisible = e.Value;
    }

    private async Task CargarTarjetasGuardadas()
    {
        try
        {
            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            if (string.IsNullOrEmpty(idUsuarioString))
            {
                await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                return;
            }

            int idUsuario = int.Parse(idUsuarioString);
            var request = new { id_usuario = idUsuario };

            using (HttpClient client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/tarjeta/consultarjetas", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var contenido = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ResObtenerTarjetas>(contenido);

                    if (resultado != null && resultado.listaDeconsultas != null && resultado.listaDeconsultas.Any())
                    {
                        foreach (var tarjeta in resultado.listaDeconsultas)
                        {
                            tarjeta.numero_trajeta = tarjeta.numero_trajeta.Length > 4
                                ? $"**** **** **** {tarjeta.numero_trajeta[^4..]}"
                                : tarjeta.numero_trajeta;
                        }

                        SavedCardsList.ItemsSource = resultado.listaDeconsultas;
                    }
                    else
                    {
                        await DisplayAlert("Sin tarjetas", "No hay tarjetas guardadas.", "Aceptar");
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
            await DisplayAlert("Error", $"Ocurrió un error al cargar las tarjetas: {ex.Message}", "Aceptar");
        }
    }

    private async void OnRegisterCardClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CardNumberEntry.Text) ||
            string.IsNullOrWhiteSpace(ExpirationDateEntry.Text) ||
            string.IsNullOrWhiteSpace(CVVEntry.Text))
        {
            await DisplayAlert("Error", "Por favor, llena todos los campos antes de registrar la tarjeta.", "Aceptar");
            return;
        }

        try
        {
            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            if (string.IsNullOrEmpty(idUsuarioString))
            {
                await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                return;
            }

            int idUsuario = int.Parse(idUsuarioString);

            var nuevaTarjeta = new ReqIngresarTarjeta
            {
                tarjetas = new InsertarTarjetas
                {
                    numero_trajeta = CardNumberEntry.Text,
                    fecha_expiracion = ExpirationDateEntry.Text,
                    CVV = CVVEntry.Text,
                    id_usuario = idUsuario
                }
            };

            using (HttpClient client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(nuevaTarjeta), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/tarjeta/ingresartarjeta", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Tarjeta registrada correctamente.", "Aceptar");
                    await CargarTarjetasGuardadas();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar la tarjeta. Intenta nuevamente.", "Aceptar");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al registrar la tarjeta: {ex.Message}", "Aceptar");
        }
    }

    private void OnCardChecked(object sender, CheckedChangedEventArgs e)
    {
        if (e.Value)
        {
            var radioButton = sender as RadioButton;

            // Obtenemos la tarjeta asociada al contexto del RadioButton
            tarjetaSeleccionada = (radioButton.BindingContext as consultaTarjeta);

            if (tarjetaSeleccionada != null)
            {
                DisplayAlert("Tarjeta seleccionada", $"Has seleccionado la tarjeta terminada en: {tarjetaSeleccionada.numero_trajeta}", "OK");
            }
        }
    }

    private async void OnPayNowClicked(object sender, EventArgs e)
    {
        if (tarjetaSeleccionada == null)
        {
            await DisplayAlert("Error", "Por favor, selecciona una tarjeta para proceder con el pago.", "Aceptar");
            return;
        }

        try
        {
            // Mostrar el indicador de carga
            LoadingGrid.IsVisible = true;
            LoadingIndicator.IsRunning = true;

            // Simular un tiempo de espera de 4 segundos
            await Task.Delay(4000);

            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            if (string.IsNullOrEmpty(idUsuarioString))
            {
                await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                return;
            }

            int idUsuario = int.Parse(idUsuarioString);
            var request = new { id_usuario = idUsuario, id_tarjeta = tarjetaSeleccionada.id_tarjeta };

            using (HttpClient client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/factura/consultarfactura", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var contenido = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ResConsultaFactura>(contenido);

                    if (resultado != null && resultado.resultado && resultado.listaDefacturas != null && resultado.listaDefacturas.Any())
                    {
                        await Navigation.PushAsync(new Factura(resultado.listaDefacturas));
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo generar la factura.", "Aceptar");
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
            await DisplayAlert("Error", $"Ocurrió un error al generar la factura: {ex.Message}", "Aceptar");
        }
        finally
        {
            // Ocultar el indicador de carga
            LoadingIndicator.IsRunning = false;
            LoadingGrid.IsVisible = false;
        }
    }


}
