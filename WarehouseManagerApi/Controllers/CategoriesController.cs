using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Category;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет категориями товаров.
    /// </summary>
    [Route("api/[controller]")]
    public class CategoriesController : ApiControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Возвращает список категорий с фильтрацией и пагинацией.
        /// </summary>
        /// <param name="filter">Параметры фильтра и пагинации.</param>
        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] CategoryFilter filter)
        {
            var result = await _categoryService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает категорию по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор категории.</param>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                return Ok(category);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Создаёт новую категорию.
        /// </summary>
        /// <param name="command">Данные создаваемой категории.</param>
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
        {
            try
            {
                var created = await _categoryService.CreateAsync(command);
                return CreatedAtAction(nameof(GetCategory), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Обновляет существующую категорию.
        /// </summary>
        /// <param name="id">Идентификатор категории.</param>
        /// <param name="command">Новые данные категории.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryCommand command)
        {
            try
            {
                var updateCommand = new UpdateCategoryCommand
                {
                    Id = id,
                    Name = command.Name,
                    Description = command.Description,
                    UserId = command.UserId
                };

                var updated = await _categoryService.UpdateAsync(updateCommand);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Архивирует категорию.
        /// </summary>
        /// <param name="id">Идентификатор категории.</param>
        /// <param name="userId">Идентификатор пользователя, выполняющего операцию.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveCategory(int id, [FromQuery] int userId)
        {
            try
            {
                await _categoryService.ArchiveAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
