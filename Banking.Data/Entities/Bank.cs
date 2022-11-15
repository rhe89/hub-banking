using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

public class Bank : EntityBase
{
    [UsedImplicitly]
    [Column]
    public string Name { get; set; }
    
    [UsedImplicitly]
    [Column]
    public string AccountNumberPrefix { get; set; }
}