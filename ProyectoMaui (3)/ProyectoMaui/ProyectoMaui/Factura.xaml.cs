using Newtonsoft.Json;
using ProyectoMaui.DTO;
using System.Text;

namespace ProyectoMaui;

public partial class Factura : ContentPage
{
    public Factura(List<Consultar> facturas)
    {
        InitializeComponent();
        AppShell.SetNavBarIsVisible(this, false);
        MostrarFactura(facturas);
    }

    private void MostrarFactura(List<Consultar> facturas)
    {
        if (facturas == null || facturas.Count == 0)
        {
            DisplayAlert("Error", "No hay datos para mostrar en la factura.", "Aceptar");
            return;
        }

        var factura = facturas.FirstOrDefault();
        if (factura != null)
        {
            // Actualizar información general
            FacturaDetalles.Children.Clear();
            FacturaDetalles.Children.Add(new Label { Text = $"Factura ID: {factura.id_factura}", FontSize = 16, TextColor = Colors.White });
            FacturaDetalles.Children.Add(new Label { Text = $"Fecha: {factura.fecha:yyyy-MM-dd}", FontSize = 16, TextColor = Colors.White });
            FacturaDetalles.Children.Add(new Label { Text = $"Nombre: {factura.nombre_usuario}", FontSize = 16, TextColor = Colors.White });
            FacturaDetalles.Children.Add(new Label { Text = $"Cédula: {factura.cedula}", FontSize = 16, TextColor = Colors.White });

            // Mostrar productos
            ProductosGrid.Children.Clear();
            int row = 1;
            foreach (var producto in facturas)
            {
                ProductosGrid.Add(new Label
                {
                    Text = producto.cantidad.ToString(),
                    TextColor = Colors.White,
                    FontSize = 14
                }, 0, row);

                ProductosGrid.Add(new Label
                {
                    Text = producto.nombre_producto,
                    TextColor = Colors.White,
                    FontSize = 14
                }, 1, row);

                ProductosGrid.Add(new Label
                {
                    Text = $"${producto.precio_producto:F2}",
                    TextColor = Colors.White,
                    FontSize = 14
                }, 2, row);

                ProductosGrid.Add(new Label
                {
                    Text = $"${producto.precio_producto * producto.cantidad:F2}",
                    TextColor = Colors.White,
                    FontSize = 14
                }, 3, row);

                row++;
            }

            // Actualizar total
            TotalLabel.Text = $"${factura.monto_total:F2}";
        }
    }
    protected override bool OnBackButtonPressed()
    {
        // Navegar a MenuPage cuando se presione el botón de "Volver"
        Navigation.PushAsync(new MenuPage());
        return true; // Evitar el comportamiento por defecto
    }
}
