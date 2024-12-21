using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using ProyectoMaui.DTO;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace ProyectoMaui;

public partial class MenuPage : ContentPage
{
    private Button _previouslySelectedButton;

    public MenuPage()
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
        CargarNombreUsuario();
        MostrarIndicadorDeCarga();
    }

    private async void MostrarIndicadorDeCarga()
    {
        LoadingGrid.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        LoadingLabel.Text = "Cargando productos...";

        await Task.Delay(3000); // Simular tiempo de carga de 3 segundos

        LoadingGrid.IsVisible = false;
        LoadingIndicator.IsRunning = false;

        // Seleccionar y cargar productos de la categoría "Capuchino"
        var capuchinoButton = this.FindByName<Button>("CapuchinoButton");
        OnCategorySelected(capuchinoButton, EventArgs.Empty);
    }

    private async void CargarNombreUsuario()
    {
        try
        {
            var nombreUsuario = await SecureStorage.GetAsync("nombre");
            UserNameLabel.Text = string.IsNullOrEmpty(nombreUsuario) ? "Hi," : $"Hi {nombreUsuario},";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al cargar el nombre del usuario: {ex.Message}", "Aceptar");
        }
    }

    private async Task<List<Producto>> ObtenerProductos()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://backendyapi.azurewebsites.net/api/producto/consultarproductos");
                if (response.IsSuccessStatusCode)
                {
                    var contenido = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ResObtenerProductos>(contenido);

                    if (resultado.Resultado)
                    {
                        return resultado.ListaDePublicaciones;
                    }
                    else
                    {
                        await DisplayAlert("Error", resultado.Error, "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo conectar con el servidor", "Aceptar");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "Aceptar");
        }
        return new List<Producto>();
    }

    private async void LoadProducts(string category)
    {
        var productos = await ObtenerProductos();
        if (productos == null || productos.Count == 0)
        {
            await DisplayAlert("Sin productos", "No hay productos disponibles.", "Aceptar");
            return;
        }

        var productosFiltrados = productos.Where(p => p.nombre_categoria == category).ToList();

        ProductosGrid.Children.Clear();
        ProductosGrid.ColumnDefinitions.Clear();
        ProductosGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ProductosGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        int column = 0;
        int row = 0;

        foreach (var producto in productosFiltrados)
        {
            string imagenLocal = BuscarImagenEnRecursos(producto.nombre);

            var frame = new Frame
            {
                BackgroundColor = Color.FromArgb("#FF333333"),
                CornerRadius = 10,
                Padding = 10,
                Margin = new Thickness(5, 10, 5, 10),
                Content = new StackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Image { Source = imagenLocal, Aspect = Aspect.AspectFill, HeightRequest = 100 },
                        new Label { Text = producto.nombre, TextColor = Colors.White, FontSize = 14, HorizontalTextAlignment = TextAlignment.Center },
                        new Label { Text = $"${producto.precio_producto:F2}", TextColor = Colors.White, FontAttributes = FontAttributes.Bold, FontSize = 16, HorizontalTextAlignment = TextAlignment.Center },
                        new Button
                        {
                            Text = "+",
                            BackgroundColor = Color.FromArgb("#8B4513"),
                            TextColor = Colors.White,
                            CornerRadius = 15,
                            Command = new Command(async () =>
                            {
                                await MostrarPopupAgregarAlCarrito(producto);
                            })
                        }
                    }
                },
                GestureRecognizers =
                {
                    new TapGestureRecognizer
                    {
                        Command = new Command(async () =>
                        {
                            await Navigation.PushAsync(new ProductoPageCLS(producto));
                        })
                    }
                }
            };

            ProductosGrid.Add(frame, column, row);
            column++;
            if (column == 2)
            {
                column = 0;
                row++;
            }
        }
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
        return "default_image.jpg";
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

    private async Task MostrarPopupAgregarAlCarrito(Producto producto)
    {
        var tiendas = await ObtenerTiendas();
        if (tiendas == null || tiendas.Count == 0)
        {
            await DisplayAlert("Error", "No hay tiendas disponibles para seleccionar.", "Aceptar");
            return;
        }

        bool isDarkMode = Application.Current.RequestedTheme == AppTheme.Dark;
        var backgroundColor = isDarkMode ? Colors.Black : Colors.White;
        var textColor = isDarkMode ? Colors.White : Colors.Black;

        int cantidad = 1;
        var cantidadLabel = new Label
        {
            Text = cantidad.ToString(),
            TextColor = textColor,
            FontAttributes = FontAttributes.Bold,
            FontSize = 18,
            HorizontalOptions = LayoutOptions.Center
        };

        var pickerSize = new Picker
        {
            ItemsSource = new List<string> { "Pequeño", "Mediano", "Grande" },
            SelectedIndex = 0,
            TextColor = textColor,
            BackgroundColor = backgroundColor
        };

        var pickerStore = new Picker
        {
            ItemsSource = tiendas,
            SelectedIndex = 0,
            TextColor = textColor,
            BackgroundColor = backgroundColor
        };

        var stepper = new Stepper
        {
            Minimum = 1,
            Maximum = 10,
            Increment = 1,
            Value = 1
        };
        stepper.ValueChanged += (s, e) =>
        {
            cantidad = (int)e.NewValue;
            cantidadLabel.Text = cantidad.ToString();
        };

        var popupFrame = new Frame
        {
            BackgroundColor = backgroundColor,
            CornerRadius = 15,
            Padding = 20,
            WidthRequest = 300,
            Content = new StackLayout
            {
                Spacing = 15,
                Children =
            {
                new Label
                {
                    Text = producto.nombre,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 20,
                    TextColor = textColor,
                    HorizontalOptions = LayoutOptions.Center
                },
                new Label { Text = "Selecciona el tamaño:", TextColor = textColor },
                pickerSize,
                new Label { Text = "Selecciona la tienda:", TextColor = textColor },
                pickerStore,
                new Label { Text = "Cantidad:", TextColor = textColor },
                cantidadLabel,
                stepper,
                new Button
                {
                    Text = "Agregar al carrito",
                    BackgroundColor = Colors.SaddleBrown,
                    TextColor = Colors.White,
                    Command = new Command(async () =>
                    {
                        var tamañoMapping = new Dictionary<string, string>
                        {
                            { "Pequeño", "Pequenho" },
                            { "Mediano", "Mediano" },
                            { "Grande", "Grande" }
                        };

                        string sizeVisible = pickerSize.SelectedItem?.ToString() ?? "Pequeño";
                        string sizeMapped = tamañoMapping.ContainsKey(sizeVisible) ? tamañoMapping[sizeVisible] : sizeVisible;

                        string store = pickerStore.SelectedItem?.ToString() ?? tiendas[0];
                        await AgregarProductoAlCarrito(producto, sizeMapped, store, cantidad);

                        // Cerrar popup después de agregar
                        await Navigation.PopModalAsync();
                    })
                }
            }
            }
        };

        var popupPage = new ContentPage
        {
            BackgroundColor = Color.FromRgba(0, 0, 0, 0.5),
            Content = new Grid
            {
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { popupFrame }
            }
        };

        await Navigation.PushModalAsync(popupPage);
    }

    private async Task AgregarProductoAlCarrito(Producto producto, string size, string store, int quantity)
    {
        var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
        if (string.IsNullOrEmpty(idUsuarioString))
        {
            await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
            return;
        }

        int idUsuario = int.Parse(idUsuarioString);
        var carrito = new AgregarC
        {
            id_usuario = idUsuario,
            nombre_producto = producto.nombre,
            cantidad = quantity,
            tamanho = size,
            nombre_tienda = store
        };

        var jsonContent = new StringContent(JsonConvert.SerializeObject(new { carrito }), Encoding.UTF8, "application/json");
        using (HttpClient client = new HttpClient())
        {
            var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/carrito/ingresarCarrito", jsonContent);
            if (response.IsSuccessStatusCode)
            {
                await DisplayAlert("Carrito", $"{producto.nombre} fue agregado al carrito correctamente.", "Aceptar");
            }
            else
            {
                await DisplayAlert("Error", "No se pudo agregar el producto al carrito.", "Aceptar");
            }
        }
    }

    private async Task<List<string>> ObtenerTiendas()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://backendyapi.azurewebsites.net/api/tienda/consultartienda");

                if (response.IsSuccessStatusCode)
                {
                    var contenido = await response.Content.ReadAsStringAsync();

                    // Deserializar la respuesta JSON
                    var resultado = JsonConvert.DeserializeObject<ResObtenerTiendas>(contenido);

                    // Validar si el resultado es exitoso
                    if (resultado.Resultado)
                    {
                        return resultado.ListaDeConsultas.Select(t => t.nombre).ToList();
                    }
                    else
                    {
                        await DisplayAlert("Error", resultado.Error, "Aceptar");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo conectar con el servidor.", "Aceptar");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al obtener tiendas: {ex.Message}", "Aceptar");
        }

        return new List<string>();
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        var button = sender as Button;
        if (_previouslySelectedButton != null)
        {
            _previouslySelectedButton.BackgroundColor = Color.FromArgb("#333");
            _previouslySelectedButton.TextColor = Colors.White;
        }

        button.BackgroundColor = Color.FromArgb("#8B4513");
        button.TextColor = Colors.White;
        _previouslySelectedButton = button;

        LoadProducts(button.Text);
    }

    private async void OnPerfilButtonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new Perfil());
    private async void OnCarritoButtonClicked(object sender, EventArgs e) => await Navigation.PushAsync(new CarritoPage());

    protected override bool OnBackButtonPressed()
    {
        return true; // Deshabilitar el botón de retroceso
    }
}
