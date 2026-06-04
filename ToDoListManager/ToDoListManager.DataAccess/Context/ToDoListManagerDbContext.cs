using Microsoft.EntityFrameworkCore;
using ToDoListManager.Model.Entities;
using Person = ToDoListManager.Model.Entities.Person;

namespace ToDoListManager.DataAccess.Context;

public class ToDoListManagerDbContext : DbContext
{
    public ToDoListManagerDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Category>? Categories { get; init; }

    public DbSet<Person>? Persons { get; init; }

    public DbSet<ToDoItem>? ToDoItems { get; init; }

    public DbSet<ToDoList>? ToDoLists { get; init; }

    public DbSet<User>? Users { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasKey(category => category.Id);
        modelBuilder.Entity<User>().HasKey(user => user.Id);
        modelBuilder.Entity<Person>().HasKey(person => person.Id);
        modelBuilder.Entity<ToDoItem>().HasKey(toDoItem => toDoItem.Id);
        modelBuilder.Entity<ToDoList>().HasKey(toDoList => toDoList.Id);

        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();

        modelBuilder
            .Entity<User>()
            .HasOne(user => user.Person)
            .WithOne(person => person.User)
            .HasForeignKey<User>(user => user.PersonId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ToDoList>()
            .HasOne(toDoList => toDoList.User)
            .WithMany(user => user.ToDoLists)
            .HasForeignKey(toDoList => toDoList.UserId)
            .HasPrincipalKey(user => user.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ToDoItem>()
            .HasOne(toDoItem => toDoItem.ToDoList)
            .WithMany(toDoList => toDoList.ToDoItems)
            .HasForeignKey(toDoItem => toDoItem.ToDoListId)
            .HasPrincipalKey(toDoList => toDoList.Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<ToDoItem>()
            .HasOne(toDoItem => toDoItem.Category)
            .WithMany(category => category.ToDoItems)
            .HasForeignKey(toDoItem => toDoItem.CategoryId)
            .HasPrincipalKey(category => category.Id)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder
            .Entity<Category>()
            .HasOne(category => category.User)
            .WithMany(user => user.Categories)
            .HasForeignKey(category => category.UserId)
            .HasPrincipalKey(user => user.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}