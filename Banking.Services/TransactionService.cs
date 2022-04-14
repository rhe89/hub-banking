﻿using System;
using System.Threading.Tasks;
using Hub.Shared.DataContracts.Banking;
using Hub.Shared.Storage.Repository.Core;
using Banking.Data.Entities;
using Banking.Providers;

namespace Banking.Services;

public interface ITransactionService
{
    Task<bool> UpdateTransaction(long transactionId, string description, DateTime date, decimal amount);
}
    
public class TransactionService : ITransactionService
{
    private readonly IHubDbRepository _hubDbRepository;
    private readonly ITransactionProvider _transactionProvider;

    public TransactionService(IHubDbRepository hubDbRepository, ITransactionProvider transactionProvider)
    {
        _hubDbRepository = hubDbRepository;
        _transactionProvider = transactionProvider;
    }
        
    public async Task<bool> UpdateTransaction(long transactionId, string description, DateTime date, decimal amount)
    {
        var transaction = await _transactionProvider.GetTransaction(transactionId);

        transaction.Description = description;
        transaction.TransactionDate = date;
        transaction.Amount = amount;

        await _hubDbRepository.UpdateAsync<Transaction, TransactionDto>(transaction);

        return true;
    }
}