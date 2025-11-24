using Microsoft.AspNetCore.Mvc;
using WarehouseManager.Services.Filters;
using WarehouseManager.Services.Services.Interfaces;
using WarehouseManagerContracts.DTOs.Product;

namespace WarehouseManagerApi.Controllers
{
    /// <summary>
    /// Управляет товарами каталога.
    /// </summary>
    [Route("api/[controller]")]
    public class ProductsController : ApiControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Возвращает список товаров с фильтрами.
        /// </summary>
        /// <param name="filter">Параметры фильтра и пагинации.</param>
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductsFilters filter)
        {
            var result = await _productService.GetPagedAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Возвращает товар по идентификатору.
        /// </summary>
        /// <param name="id">Идентификатор товара.</param>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                return Ok(product);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Создаёт новый товар.
        /// </summary>
        /// <param name="command">Данные нового товара.</param>
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
        {
            try
            {
                var created = await _productService.CreateAsync(command);
                return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Обновляет сведения о товаре.
        /// </summary>
        /// <param name="id">Идентификатор товара.</param>
        /// <param name="command">Изменяемые параметры.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
        {
            try
            {
                var updateCommand = command with { Id = id };
                var updated = await _productService.UpdateAsync(updateCommand);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Архивирует товар.
        /// </summary>
        /// <param name="id">Идентификатор товара.</param>
        /// <param name="userId">Пользователь, выполняющий операцию.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> ArchiveProduct(int id, [FromQuery] int userId)
        {
            try
            {
                await _productService.ArchiveAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return HandleException(ex);
            }
        }
    }
}
