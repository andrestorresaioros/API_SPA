using Application.Common.Models;
using Application.Products.Dtos;

namespace Application.Products.Services;

public interface IProductService
{
    Task<ProductDetailResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    Task<BulkCreateProductsResponse> CreateBulkAsync(BulkCreateProductsRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<ProductListItemResponse>> GetAllAsync(ProductsQueryRequest request, CancellationToken cancellationToken = default);

    Task<ProductDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ProductDetailResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
