namespace SistemaVetIng.Servicios.Interfaces
{
    public interface IMercadoPagoService
    {
        Task<string> CrearPreferenciaDePago(int idReferencia, decimal costoTotal, string clienteEmail, long clienteDocumento, string clienteNombre, string clienteApellido);
    }
}
