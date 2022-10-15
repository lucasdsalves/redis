using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace redis.Controllers;

[ApiController]
[Route("[controller]")]
public class ToDosController : ControllerBase
{
    private readonly ILogger<ToDosController> _logger;
    private readonly ToDoContext _context;
    private readonly ICachingService _cache;

    public ToDosController(ILogger<ToDosController> logger, ToDoContext context, ICachingService cache)
    {
        _logger = logger;
        _context = context;
        _cache = cache;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var todoFromCache = await _cache.GetAsync(id.ToString());
        ToDo? todo;

        if (!string.IsNullOrWhiteSpace(todoFromCache))
        {
            todo = JsonConvert.DeserializeObject<ToDo>(todoFromCache);

            _logger.LogInformation("ToDo was loaded from cache.");

            return Ok(todo);
        }

        todo = await _context.ToDos.SingleOrDefaultAsync(t => t.Id == id);

        if (todo == null) return NotFound();

        await _cache.SetAsync(id.ToString(), JsonConvert.SerializeObject(todo));
        _logger.LogInformation("ToDo was sent to Redis cache.");

        return Ok(todo);
    }

    [HttpPost]
    public async Task<IActionResult> Post(ToDoModel model)
    {
        var todo = new ToDo(0, model.Description);

        await _context.ToDos.AddAsync(todo);
        await _context.SaveChangesAsync();

        _logger.LogInformation("A new ToDo was created.");
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, model);
    }
}
