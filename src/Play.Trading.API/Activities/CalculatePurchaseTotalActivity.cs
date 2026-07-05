using System;
using System.Threading.Tasks;
using Automatonymous;
using GreenPipes;
using Play.Common.Repositories;
using Play.Trading.API.Contracts;
using Play.Trading.API.Entities;
using Play.Trading.API.Exceptions;
using Play.Trading.API.StateMachines;

namespace Play.Trading.API.Activities;

public class CalculatePurchaseTotalActivity : Activity<PurchaseState, PurchaseRequested>
{
    private readonly IRepository<CatalogItem> _repository;

    public CalculatePurchaseTotalActivity(IRepository<CatalogItem> repository)
    {
        _repository = repository;
    }

    public void Probe(ProbeContext context)
    {
        context.CreateScope("calculate-purchase-total");
    }

    public void Accept(StateMachineVisitor visitor)
    {
        visitor.Visit(this);
    }

    public async Task Execute(
        BehaviorContext<PurchaseState, PurchaseRequested> context, 
        Behavior<PurchaseState, PurchaseRequested> next)
    {
        var message = context.Data;

        var item = await _repository.GetAsync(message.ItemId);
        if (item == null)
        {
            throw new UnknownItemException(message.ItemId);
        }

        context.Instance.PurchaseTotal = item.Price * message.Quantity;
        context.Instance.LastUpdated = DateTimeOffset.UtcNow;
        
        await next.Execute(context).ConfigureAwait(false);
    }

    public Task Faulted<TException>(
        BehaviorExceptionContext<PurchaseState, PurchaseRequested, TException> context, 
        Behavior<PurchaseState, PurchaseRequested> next
        ) where TException : Exception
    {
        return next.Faulted(context);
    }
}