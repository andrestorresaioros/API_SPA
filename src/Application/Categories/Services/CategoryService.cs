using Application.Abstractions.Persistence;
using Application.Categories.Dtos;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _dbContext;

    public CategoryService(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.CategoryName.Trim();

        var exists = await _dbContext.Categories
            .AsNoTracking()
            .AnyAsync(category => category.CategoryName.ToLower() == name.ToLower(), cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException("A category with the same name already exists.");
        }

        var category = new Category
        {
            CategoryName = name,
            Description = request.Description?.Trim(),
            Picture = request.Picture
        };

        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(category);
    }

    public async Task<IReadOnlyCollection<CategoryResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _dbContext.Categories
            .AsNoTracking()
            .OrderBy(category => category.CategoryName)
            .ToListAsync(cancellationToken);

        return categories.Select(MapToResponse).ToList();
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description,
            Picture = category.Picture
        };
    }
}
