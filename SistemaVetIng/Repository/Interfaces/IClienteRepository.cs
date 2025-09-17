using Microsoft.EntityFrameworkCore;
using SistemaVetIng.Models;

namespace SistemaVetIng.Repository.Interfaces
{
    public interface IClienteRepository 
    {
        Task Agregar(Cliente entity);


        void Eliminar(Cliente entity);
            

        Task Guardar();
        Task<IEnumerable<Cliente>> ListarTodo();

        void Modificar(Cliente entity);

        Task<Cliente> ObtenerPorId(int id);

        Task<Cliente> ObtenerPorIdUsuario(int UsuarioId);
    }
}
