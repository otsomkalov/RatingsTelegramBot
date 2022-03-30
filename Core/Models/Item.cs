﻿namespace Core.Models;

public record Item : BaseEntity
{
    public virtual Category Category { get; init; }

    public int? CategoryId { get; init; }

    public virtual Manufacturer Manufacturer { get; init; }

    public int? ManufacturerId { get; init; }

    public string Name { get; init; }

    public virtual Place Place { get; init; }

    public int? PlaceId { get; init; }

    public virtual IReadOnlyCollection<Rating> Ratings { get; init; }
}