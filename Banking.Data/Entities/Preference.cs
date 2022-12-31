using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

[UsedImplicitly]
public class Preference : EntityBase
{
    [UsedImplicitly]
    [Column]
    public string Key { get; set; }
    
    [UsedImplicitly]
    [Column]
    public string Value { get; set; }
}