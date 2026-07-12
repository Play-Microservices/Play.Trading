using System;
using Play.Common.Entites;

namespace Play.Trading.API.Entities;

public class ApplicationUser : IEntity
{
    public Guid Id { get; set; }
    public decimal Gil { get; set; }
}