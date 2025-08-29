namespace SistemaVetIng.Repository.Interfaces
{
    public interface IGeneralRepository<T>
    {
        Task<IEnumerable<T>> ListarTodo();
        Task<T> ObtenerPorId(int id);
        Task Agregar(T entity);
        void Modificar(T entity);
        void Eliminar(T entity);
        Task Guardar();
    }
}
