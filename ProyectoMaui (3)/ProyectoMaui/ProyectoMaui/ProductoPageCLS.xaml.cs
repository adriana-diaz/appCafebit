using Microsoft.Maui.Controls;
using ProyectoMaui.DTO;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace ProyectoMaui;

public partial class ProductoPageCLS : ContentPage
{
    private Producto _producto;
    private int quantity = 1;

    public ProductoPageCLS(Producto producto)
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);

        _producto = producto;
        CargarInformacionProducto();
    }

    private void CargarInformacionProducto()
    {
        // Asignar información al producto
        ProductoNombre.Text = _producto.nombre;
        ProductoPrecio.Text = $"${_producto.precio_producto:F2}";
        ProductoDescripcion.Text = _producto.descripcion;

        // Buscar y asignar la imagen del producto
        string imagenLocal = BuscarImagenEnRecursos(_producto.nombre);
        ProductoImagen.Source = imagenLocal; // Control Image en XAML
    }

    private void OnDecreaseClicked(object sender, EventArgs e)
    {
        if (quantity > 1)
        {
            quantity--;
            QuantityLabel.Text = quantity.ToString();
        }
    }

    private void OnIncreaseClicked(object sender, EventArgs e)
    {
        quantity++;
        QuantityLabel.Text = quantity.ToString();
    }

    private async void OnAgregarAlCarritoClicked(object sender, EventArgs e)
    {
        string selectedSize = GetSelectedSize() ?? "Pequenho";

        try
        {
            var idUsuarioString = await SecureStorage.GetAsync("id_usuario");
            if (string.IsNullOrEmpty(idUsuarioString))
            {
                await DisplayAlert("Error", "No se encontró el usuario en la sesión. Por favor, inicie sesión nuevamente.", "Aceptar");
                return;
            }

            int idUsuario = int.Parse(idUsuarioString);
            var tiendas = await ObtenerTiendas();
            if (tiendas == null || tiendas.Count == 0)
            {
                await DisplayAlert("Error", "No hay tiendas disponibles para seleccionar.", "Aceptar");
                return;
            }

            string tiendaSeleccionada = await DisplayActionSheet("Selecciona una tienda", "Cancelar", null, tiendas.ToArray());
            if (string.IsNullOrEmpty(tiendaSeleccionada) || tiendaSeleccionada == "Cancelar")
            {
                return;
            }

            var carrito = new AgregarC
            {
                id_usuario = idUsuario,
                nombre_producto = _producto.nombre,
                cantidad = quantity,
                tamanho = selectedSize,
                nombre_tienda = tiendaSeleccionada
            };

            var jsonContent = new StringContent(JsonConvert.SerializeObject(new { carrito }), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync("https://backendyapi.azurewebsites.net/api/carrito/ingresarCarrito", jsonContent);
                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Carrito", $"{quantity} {selectedSize} {_producto.nombre} fue agregado al carrito desde la tienda {tiendaSeleccionada}.", "Aceptar");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo agregar el producto al carrito. Intenta nuevamente.", "Aceptar");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Ocurrió un error al agregar el producto al carrito: {ex.Message}", "Aceptar");
        }
    }

    private string GetSelectedSize()
    {
        foreach (var child in MenuCategorias.Children)
        {
            if (child is RadioButton radioButton && radioButton.IsChecked)
            {
                var size = radioButton.Content?.ToString();
                return size == "Pequeño" ? "Pequenho" : size;
            }
        }
        return null;
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
                    var resultado = JsonConvert.DeserializeObject<ResObtenerTiendas>(contenido);

                    if (resultado.Resultado)
                    {
                        return resultado.ListaDeConsultas.Select(t => t.nombre).ToList();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener tiendas: {ex.Message}");
        }
        return new List<string>();
    }

    private string BuscarImagenEnRecursos(string nombreBase)
    {
        // Normalizar el nombre del producto
        string nombreNormalizado = NormalizarTexto(nombreBase);

        var extensiones = new[] { "jpg", "jpeg", "png" };

        foreach (var extension in extensiones)
        {
            string nombreArchivo = $"{nombreNormalizado}.{extension}";
            try
            {
                var imageSource = ImageSource.FromFile(nombreArchivo);
                return nombreArchivo; // Si existe, devolver el nombre
            }
            catch { }
        }

        return "default_image.jpg"; // Imagen por defecto
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

    private void OnHomeButtonClicked(object sender, EventArgs e) => Navigation.PushAsync(new MenuPage());
    private void OnCartButtonClicked(object sender, EventArgs e) => Navigation.PushAsync(new CarritoPage());
    private void OnUserButtonClicked(object sender, EventArgs e) => Navigation.PushAsync(new Perfil());
}
