using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoService _todoService;

        public TodoController(TodoService todoService)
        {
            _todoService = todoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<TodoItem>>> Get() =>
            await _todoService.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> Get(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ObjectId format");

            var todo = await _todoService.GetAsync(id);
            return todo is null ? NotFound() : todo;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TodoItem todoItem)
        {
            if (string.IsNullOrWhiteSpace(todoItem.Title) || string.IsNullOrWhiteSpace(todoItem.Description))
                return BadRequest("Title and Description are required");

            todoItem.Id = null; // Ensure MongoDB auto-generates it
            await _todoService.CreateAsync(todoItem);
            return CreatedAtAction(nameof(Get), new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] TodoItem updatedItem)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ObjectId format");

            var existing = await _todoService.GetAsync(id);
            if (existing is null)
                return NotFound();

            updatedItem.Id = id;
            await _todoService.UpdateAsync(id, updatedItem);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Invalid ObjectId format");

            var existing = await _todoService.GetAsync(id);
            if (existing is null)
                return NotFound();

            await _todoService.DeleteAsync(id);
            return NoContent();
        }
    }
}
