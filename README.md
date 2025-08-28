Points two and three are answered here.

2. For data that changes little but is consulted all the time, cache memory could be used. In a single instance, there would be no problem, but if the application runs in multiple instances, there would be problems with data desynchronisation.

3.

public void UpdateCustomersBalanceByInvoices(List<Invoice> invoices)
{
    //1. parameter was not validate
    if (invoices == null || invoices.Count == 0) throw new ArgumentNullException(nameof(invoices));

    //2. We see that the idea is to update the customer balance, so it could be optimised by grouping invoices by customer and adding the total to be 
    // subtracted in a single operation per customer.
    var customerBalancesToUpdate = invoices
        .Where(inv => inv.CustomerId != null)
        .GroupBy(inv => inv.CustomerId.Value)
        .Select(g => new { CustomerId = g.Key, Total = g.Sum(inv => inv.Total) })
        .ToList();

    //3. i can search users involved in invoices so that i don't have to query each invoice in the foreach

    var customerIds = customerBalancesToUpdate.Select(cb => cb.CustomerId).ToList();
    var customers = dbContext.Customers.Where(c => customerIds.Contains(c.Id)).ToList();

    //4. In the foreach, a query is being made for each invoice, which can be inefficient. Instead, a foreach will be performed on the grouped balances and a single saveChanges will be performed at the end.

    foreach (var item in customerBalancesToUpdate)
    {
        var customer = customers.FirstOrDefault(c => c.Id == item.CustomerId);
        customer.Balance -= item.Total;
    }

    dbContext.SaveChanges();
}
