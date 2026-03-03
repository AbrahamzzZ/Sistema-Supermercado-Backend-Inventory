namespace Domain.Model;
    public class MovimientoStock
    {
        public int IdMovimientoStock { get; set; }
        public int IdProducto { get; set; }
        public string? Tipo { get; set; }
        public int Cantidad { get; set; }
        public string? Referencia { get; set; }
        public DateTime? Fecha{ get; set; }
    }
