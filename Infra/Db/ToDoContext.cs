using Microsoft.EntityFrameworkCore;

public class ToDoContext : DbContext{
    public ToDoContext(DbContextOptions<ToDoContext> options) : base(options){
    }

    public DbSet<ToDo> ToDos { get; set; }

    protected override void OnModelCreating(ModelBuilder builder){
        builder.Entity<ToDo>().HasKey(t => t.Id);
    }
}