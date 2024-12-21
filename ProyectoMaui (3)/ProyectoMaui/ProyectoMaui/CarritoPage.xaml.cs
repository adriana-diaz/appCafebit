using Newtonsoft.Json;
using ProyectoMaui.DTO;
using System.Globalization;
using System.Text;

namespace ProyectoMaui;

public partial class CarritoPage : ContentPage
{
    public CarritoPage()
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
        CargarProductosCarrito();
    }

    private async Task CargarProductosCarrito()
    {
        try
        {
            // Mostrar el indicador de carga
            ActivityIndicator.IsRunning = true;
            ActivityIndicator.IsVisible = true;

            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            if (string.IsNullOrEmpty(idUsuarioString))
            {
                await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                return;
            }

            int idUsuario = int.Parse(idUsuarioString);
            var request = new ReqObtenerProductosCarrito { id_usuario = idUsuario };

            using (HttpClient client = new HttpClient())
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/carrito/consultandoCarrito", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var contenido = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ResObtenerProductosCarrito>(contenido);

                    if (resultado != null && resultado.Resultado && resultado.listaDeconsultas != null && resultado.listaDeconsultas.Any())
                    {
                        LlenarVistaCarrito(resultado.listaDeconsultas);
                    }
                    else
                    {
                        // Si no hay productos, limpiar la vista
                        CarritoStack.Children.Clear();
                        TotalLabel.Text = "$0.00";
                        await DisplayAlert("Carrito vacío", "No hay productos en el carrito.", "Aceptar");
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
            await DisplayAlert("Error", $"Ocurrió un error al cargar el carrito: {ex.Message}", "Aceptar");
        }
        finally
        {
            // Ocultar el indicador de carga
            ActivityIndicator.IsRunning = false;
            ActivityIndicator.IsVisible = false;
        }
    }


    private void LlenarVistaCarrito(List<consulta> productos)
    {
        CarritoStack.Children.Clear();
        decimal total = 0;

        foreach (var producto in productos)
        {
            total += producto.precio_producto * producto.cantidad;

            var grid = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Auto }, // Columna para la imagen
                new ColumnDefinition { Width = GridLength.Star }  // Columna para el texto
            },
                RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto } // Fila única
            }
            };

            // Buscar la imagen correspondiente al producto
            string imagenLocal = BuscarImagenEnRecursos(producto.nombre_producto);

            // Imagen
            var image = new Image
            {
                Source = imagenLocal, // Usar la imagen encontrada
                WidthRequest = 80,
                HeightRequest = 80,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Center
            };
            Grid.SetColumn(image, 0); // Establecer columna 0
            grid.Children.Add(image);

            // Información del producto
            var stackLayout = new StackLayout
            {
                Padding = new Thickness(10, 0),
                Children =
            {
                new Label { Text = producto.nombre_producto, FontAttributes = FontAttributes.Bold, FontSize = 16, TextColor = Colors.White },
                new Label { Text = producto.descripcion, FontSize = 14, TextColor = Colors.Gray },
                new Label { Text = $"Cantidad: {producto.cantidad}", FontSize = 14, TextColor = Colors.White },
                new Label { Text = $"Tamaño: {producto.tamanho}", FontSize = 14, TextColor = Colors.White }
            }
            };
            Grid.SetColumn(stackLayout, 1); // Establecer columna 1
            grid.Children.Add(stackLayout);

            // Botón para eliminar producto
            var deleteButton = new Button
            {
                Text = "✖",
                BackgroundColor = Colors.Transparent,
                TextColor = Colors.Red,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Command = new Command(() => EliminarProducto(producto.id_carrito ?? 0))
            };
            Grid.SetColumn(deleteButton, 1); // Establecer columna 1
            grid.Children.Add(deleteButton);

            // Crear el frame para el producto
            var frame = new Frame
            {
                Padding = 10,
                Margin = new Thickness(0, 5),
                BackgroundColor = Color.FromArgb("#1A1A1A"),
                CornerRadius = 8,
                Content = grid
            };

            CarritoStack.Children.Add(frame);
        }

        // Actualizar el total
        TotalLabel.Text = $"${total:F2}";
    }
    private string BuscarImagenEnRecursos(string nombreBase)
    {
        string nombreNormalizado = NormalizarTexto(nombreBase);
        var extensiones = new[] { "jpg", "jpeg", "png" };

        foreach (var extension in extensiones)
        {
            string nombreArchivo = $"{nombreNormalizado}.{extension}";
            try
            {
                var imageSource = ImageSource.FromFile(nombreArchivo);
                if (imageSource != null)
                {
                    return nombreArchivo;
                }
            }
            catch { }
        }
        return "default_image.jpg"; // Imagen predeterminada si no se encuentra ninguna
    }

    private string NormalizarTexto(string texto)
    {
        texto = texto.ToLowerInvariant();
        texto = new string(texto
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .Normalize(NormalizationForm.FormC);
        return texto.Replace(" ", "_");
    }



    private async void EliminarProducto(int idCarrito)
    {
        if (idCarrito == 0) return;

        bool confirmar = await DisplayAlert("Eliminar producto", "¿Desea eliminar este producto del carrito?", "Sí", "No");
        if (!confirmar) return;

        try
        {
            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            if (string.IsNullOrEmpty(idUsuarioString))
            {
                await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                return;
            }

            int idUsuario = int.Parse(idUsuarioString);

            var eliminarCarritoRequest = new
            {
                carrito = new EliminarC
                {
                    id_usuario = idUsuario,
                    id_carrito = idCarrito
                }
            };

            using (HttpClient client = new HttpClient())
            {
                var requestUrl = "https://backendyapi.azurewebsites.net/api/carrito/eliminarProductodelCarrito";
                var jsonContent = new StringContent(JsonConvert.SerializeObject(eliminarCarritoRequest), Encoding.UTF8, "application/json");

                ActivityIndicator.IsRunning = true;
                ActivityIndicator.IsVisible = true;

                var response = await client.PostAsync(requestUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Producto eliminado del carrito.", "Aceptar");
                    await CargarProductosCarrito(); // Refrescar la lista
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo eliminar el producto. Intenta nuevamente.", "Aceptar");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al eliminar el producto: {ex.Message}", "Aceptar");
        }
        finally
        {
            ActivityIndicator.IsRunning = false;
            ActivityIndicator.IsVisible = false;
        }
    }




    private async void OnPagarClicked(object sender, EventArgs e)
    {
        var total = TotalLabel.Text; // Obtén el total como texto
        await Navigation.PushAsync(new TarjetaPage(total));
    }
}
