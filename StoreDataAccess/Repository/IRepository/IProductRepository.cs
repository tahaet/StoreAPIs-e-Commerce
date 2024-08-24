using StoreModels;

namespace StoreDataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
        Task<List<Product>> GetAllProductsWithMainImage();
    }
}
