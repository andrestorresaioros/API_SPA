using Application.Abstractions.Persistence;
using Application.Common.Models;
using Application.Products.Dtos;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Application.Products.Services;

public class ProductService : IProductService
{
    private readonly IApplicationDbContext _dbContext;

    public ProductService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDetailResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        var product = new Product
        {
            ProductName = request.ProductName.Trim(),
            CategoryId = request.CategoryId,
            QuantityPerUnit = request.QuantityPerUnit?.Trim(),
            UnitPrice = request.UnitPrice,
            UnitsInStock = request.UnitsInStock,
            UnitsOnOrder = request.UnitsOnOrder,
            ReorderLevel = request.ReorderLevel,
            Discontinued = request.Discontinued
        };

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(product, category);
    }

    public async Task<BulkCreateProductsResponse> CreateBulkAsync(BulkCreateProductsRequest request, CancellationToken cancellationToken = default)
    {
        var categories = await ResolveTargetCategoriesAsync(request, cancellationToken);
        var stopwatch = Stopwatch.StartNew();
        var createdByCategory = categories.ToDictionary(category => category.Id, _ => 0);
        var totalCreated = 0;
        var sequence = 0;

        var normalizedBatchSize = Math.Min(request.BatchSize, request.Quantity);

        if (_dbContext is DbContext efContext)
        {
            var previousAutoDetect = efContext.ChangeTracker.AutoDetectChangesEnabled;
            efContext.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                totalCreated = await InsertBatchesAsync(
                    request,
                    categories,
                    createdByCategory,
                    normalizedBatchSize,
                    sequence,
                    cancellationToken);
            }
            finally
            {
                efContext.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
            }
        }
        else
        {
            totalCreated = await InsertBatchesAsync(
                request,
                categories,
                createdByCategory,
                normalizedBatchSize,
                sequence,
                cancellationToken);
        }

        stopwatch.Stop();

        var categoriesUsed = categories
            .Select(category => new BulkCategoryUsageResponse
            {
                CategoryId = category.Id,
                CategoryName = category.CategoryName,
                CreatedCount = createdByCategory[category.Id]
            })
            .ToList();

        return new BulkCreateProductsResponse
        {
            RequestedCount = request.Quantity,
            CreatedCount = totalCreated,
            BatchSize = normalizedBatchSize,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            CategoriesUsed = categoriesUsed
        };
    }

    public async Task<PagedResult<ProductListItemResponse>> GetAllAsync(ProductsQueryRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = request.Search.Trim().ToLower();
            query = query.Where(product => product.ProductName.ToLower().Contains(normalizedSearch));
        }

        if (request.CategoryId.HasValue)
        {
            query = query.Where(product => product.CategoryId == request.CategoryId.Value);
        }

        if (request.MinPrice.HasValue)
        {
            query = query.Where(product => product.UnitPrice >= request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            query = query.Where(product => product.UnitPrice <= request.MaxPrice.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(product => product.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(product => new ProductListItemResponse
            {
                Id = product.Id,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category != null ? product.Category.CategoryName : string.Empty,
                UnitPrice = product.UnitPrice,
                UnitsInStock = product.UnitsInStock,
                Discontinued = product.Discontinued
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemResponse>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<ProductDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Include(item => item.Category)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null || product.Category is null)
        {
            return null;
        }

        return MapToDetail(product, product.Category);
    }

    public async Task<ProductDetailResponse?> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.CategoryId, cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        product.ProductName = request.ProductName.Trim();
        product.CategoryId = request.CategoryId;
        product.QuantityPerUnit = request.QuantityPerUnit?.Trim();
        product.UnitPrice = request.UnitPrice;
        product.UnitsInStock = request.UnitsInStock;
        product.UnitsOnOrder = request.UnitsOnOrder;
        product.ReorderLevel = request.ReorderLevel;
        product.Discontinued = request.Discontinued;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDetail(product, category);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (product is null)
        {
            return false;
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<int> InsertBatchesAsync(
        BulkCreateProductsRequest request,
        IReadOnlyList<Category> categories,
        Dictionary<int, int> createdByCategory,
        int batchSize,
        int sequence,
        CancellationToken cancellationToken)
    {
        var created = 0;
        var random = Random.Shared;

        for (var offset = 0; offset < request.Quantity; offset += batchSize)
        {
            var currentBatchSize = Math.Min(batchSize, request.Quantity - offset);
            var batch = new List<Product>(currentBatchSize);

            for (var index = 0; index < currentBatchSize; index++)
            {
                sequence++;

                var category = request.UseRoundRobin
                    ? categories[(offset + index) % categories.Count]
                    : categories[random.Next(categories.Count)];

                batch.Add(GenerateRandomProduct(category.Id, sequence, random));
                createdByCategory[category.Id]++;
            }

            _dbContext.Products.AddRange(batch);
            await _dbContext.SaveChangesAsync(cancellationToken);

            created += currentBatchSize;

            if (_dbContext is DbContext efContext)
            {
                // Clear tracked entities to avoid memory growth in very large loads.
                efContext.ChangeTracker.Clear();
            }
        }

        return created;
    }

    private async Task<List<Category>> ResolveTargetCategoriesAsync(
        BulkCreateProductsRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CategoryIds is { Count: > 0 })
        {
            var requestedIds = request.CategoryIds.Distinct().ToList();

            var categoriesByIds = await _dbContext.Categories
                .AsNoTracking()
                .Where(category => requestedIds.Contains(category.Id))
                .OrderBy(category => category.Id)
                .ToListAsync(cancellationToken);

            if (categoriesByIds.Count != requestedIds.Count)
            {
                var foundIds = categoriesByIds.Select(category => category.Id).ToHashSet();
                var missingIds = requestedIds.Where(id => !foundIds.Contains(id));
                throw new KeyNotFoundException($"Category IDs not found: {string.Join(", ", missingIds)}.");
            }

            return categoriesByIds;
        }

        var preferredCategories = await _dbContext.Categories
            .AsNoTracking()
            .Where(category =>
                category.CategoryName.ToLower() == "servidores"
                || category.CategoryName.ToLower() == "cloud")
            .OrderBy(category => category.Id)
            .ToListAsync(cancellationToken);

        if (preferredCategories.Count > 0)
        {
            return preferredCategories;
        }

        var fallbackCategories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.Id)
            .ToListAsync(cancellationToken);

        if (fallbackCategories.Count == 0)
        {
            throw new KeyNotFoundException("No categories were found. Create at least one category before bulk loading.");
        }

        return fallbackCategories;
    }

    private static Product GenerateRandomProduct(int categoryId, int sequence, Random random)
    {
        var productName = $"PRD-{DateTime.UtcNow:yyyyMMddHHmmss}-{sequence:D6}-{random.Next(1000, 9999)}";

        var unitsInStock = (short)random.Next(0, 500);
        var unitsOnOrder = (short)random.Next(0, 120);

        return new Product
        {
            ProductName = productName,
            CategoryId = categoryId,
            QuantityPerUnit = $"{random.Next(1, 60)} unidades",
            UnitPrice = decimal.Round((decimal)(random.NextDouble() * 9000 + 100), 2),
            UnitsInStock = unitsInStock,
            UnitsOnOrder = unitsOnOrder,
            ReorderLevel = (short)random.Next(0, 50),
            Discontinued = random.Next(0, 100) < 3
        };
    }

    private static ProductDetailResponse MapToDetail(Product product, Category category)
    {
        return new ProductDetailResponse
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CategoryId = product.CategoryId,
            QuantityPerUnit = product.QuantityPerUnit,
            UnitPrice = product.UnitPrice,
            UnitsInStock = product.UnitsInStock,
            UnitsOnOrder = product.UnitsOnOrder,
            ReorderLevel = product.ReorderLevel,
            Discontinued = product.Discontinued,
            Category = new ProductCategoryResponse
            {
                Id = category.Id,
                CategoryName = category.CategoryName,
                Description = category.Description,
                Picture = category.Picture
            }
        };
    }
}
