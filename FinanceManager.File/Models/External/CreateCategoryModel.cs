﻿namespace FinanceManager.File.Models.External;

public class CreateCategoryModel
{
    public string Name { get; set; } = null!;
    public CategoryType Type { get; set; }
    public Guid? ParentId { get; set; }
    public string? Description { get; set; }
}