﻿using System.ComponentModel.DataAnnotations;

namespace PSI_Project.Models;

// 1: using our own class
public class BaseEntity
{
    [Key]
    public string Id { get; init; } = Guid.NewGuid().ToString();
}
