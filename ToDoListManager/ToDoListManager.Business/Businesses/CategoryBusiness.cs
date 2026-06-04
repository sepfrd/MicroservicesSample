using System.Net;
using Humanizer;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ToDoListManager.Business.Contracts;
using ToDoListManager.Common.Constants;
using ToDoListManager.Common.Dtos;
using ToDoListManager.DataAccess.Contracts;
using ToDoListManager.Model.Entities;

namespace ToDoListManager.Business.Businesses;

public class CategoryBusiness : ICategoryBusiness
{
    private readonly IAuthBusiness _authBusiness;
    private readonly IBaseRepository<Category> _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryBusiness(IUnitOfWork unitOfWork, IAuthBusiness authBusiness)
    {
        _unitOfWork = unitOfWork;
        _authBusiness = authBusiness;
        _categoryRepository = unitOfWork.CategoryRepository;
    }

    public async Task<CustomResponse<CategoryDto?>> GetCategoryByGuidAsync(Guid categoryGuid, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByGuidAsync(categoryGuid, null, cancellationToken);

        if (category is null)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(
                HttpStatusCode.BadRequest,
                string.Format(
                    MessageConstants.EntityNotFound,
                    nameof(Category).Humanize(LetterCasing.Title)));
        }

        var loggedInUser = await _authBusiness.GetLoggedInUserAsync(cancellationToken);

        if (loggedInUser!.Id != category.UserId)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(HttpStatusCode.Forbidden);
        }

        var categoryDto = category.Adapt<CategoryDto>();

        return CustomResponse<CategoryDto?>.CreateSuccessfulResponse(categoryDto);
    }

    public async Task<CustomResponse<List<CategoryDto>?>> GetUserCategoriesAsync(PaginationDto? paginationDto = null, CancellationToken cancellationToken = default)
    {
        var loggedInUser = await _authBusiness.GetLoggedInUserAsync(cancellationToken);

        if (loggedInUser is null)
        {
            return CustomResponse<List<CategoryDto>?>.CreateUnsuccessfulResponse(HttpStatusCode.Unauthorized);
        }

        paginationDto ??= PaginationDto.DefaultPaginationDto;

        var categories = await _categoryRepository.GetAllAsync(
            paginationDto,
            category => category.UserId == loggedInUser.Id,
            null,
            cancellationToken);

        var categoryDtos = categories.Adapt<List<CategoryDto>>();

        return CustomResponse<List<CategoryDto>?>.CreateSuccessfulResponse(categoryDtos);
    }

    public async Task<CustomResponse<CategoryDto?>> CreateCategoryAsync(
        CreateOrUpdateCategoryDto createOrUpdateCategoryDto,
        CancellationToken cancellationToken = default)
    {
        var loggedInUser = await _authBusiness.GetLoggedInUserAsync(cancellationToken);

        if (loggedInUser is null)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(HttpStatusCode.Forbidden);
        }

        var category = createOrUpdateCategoryDto.Adapt<Category>();

        category.UserId = loggedInUser.Id;

        var createdCategory = await _categoryRepository.CreateAsync(category, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        var createdCategoryDto = createdCategory.Adapt<CategoryDto>();

        createdCategoryDto.UserGuid = loggedInUser.Guid;

        return CustomResponse<CategoryDto?>.CreateSuccessfulResponse(
            createdCategoryDto,
            string.Format(MessageConstants.SuccessfullyCreated, nameof(Category).Humanize(LetterCasing.Title)),
            HttpStatusCode.Created);
    }

    public async Task<CustomResponse<CategoryDto?>> UpdateCategoryAsync(
        Guid categoryGuid,
        CreateOrUpdateCategoryDto createOrUpdateCategoryDto,
        CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByGuidAsync(categoryGuid, null, cancellationToken);

        if (category is null)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(
                HttpStatusCode.BadRequest,
                string.Format(
                    MessageConstants.EntityNotFound,
                    nameof(Category).Humanize(LetterCasing.Title)));
        }

        var loggedInUser = await _authBusiness.GetLoggedInUserAsync(cancellationToken);

        if (loggedInUser is null)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(HttpStatusCode.Forbidden);
        }

        createOrUpdateCategoryDto.Adapt(category);

        var updatedCategory = _categoryRepository.Update(category);

        await _unitOfWork.CommitAsync(cancellationToken);

        var updateCategoryDto = updatedCategory.Adapt<CategoryDto>();

        return CustomResponse<CategoryDto?>.CreateSuccessfulResponse(
            updateCategoryDto,
            string.Format(
                MessageConstants.SuccessfullyUpdated,
                nameof(Category).Humanize(LetterCasing.Title)));
    }

    public async Task<CustomResponse<CategoryDto?>> DeleteCategoryByGuidAsync(Guid categoryGuid, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.GetByGuidAsync(
            categoryGuid,
            categories => categories.Include(categoryToDelete => categoryToDelete.ToDoItems),
            cancellationToken);

        if (category is null)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(
                HttpStatusCode.BadRequest,
                string.Format(
                    MessageConstants.EntityNotFound,
                    nameof(Category).Humanize(LetterCasing.Title)));
        }

        var loggedInUser = await _authBusiness.GetLoggedInUserAsync(cancellationToken);

        if (loggedInUser!.Id != category.UserId)
        {
            return CustomResponse<CategoryDto?>.CreateUnsuccessfulResponse(HttpStatusCode.Forbidden);
        }

        foreach (var toDoItem in category.ToDoItems)
        {
            toDoItem.CategoryId = null;
            _unitOfWork.ToDoItemRepository.Update(toDoItem);
        }

        var deletedCategory = _categoryRepository.Delete(category);

        await _unitOfWork.CommitAsync(cancellationToken);

        var deletedCategoryDto = deletedCategory.Adapt<CategoryDto>();

        return CustomResponse<CategoryDto?>.CreateSuccessfulResponse(
            deletedCategoryDto,
            string.Format(
                MessageConstants.SuccessfullyDeleted,
                nameof(Category).Humanize(LetterCasing.Title)));
    }
}