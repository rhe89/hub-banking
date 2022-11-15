using System.ComponentModel.DataAnnotations.Schema;
using Hub.Shared.Storage.Repository.Core;
using JetBrains.Annotations;

namespace Banking.Data.Entities;

public class CsvImport : EntityBase
{
    [UsedImplicitly]
    [Column]
    public string FileName { get; set; }
}