using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProblemAssignment3.Entities;
using ProblemAssignment3.Models;

namespace ProblemAssignment3.Controllers
{
    public class CustomersController : Controller
    {
        private CustomerInvoiceDBContext _dbContext;
        public CustomersController(CustomerInvoiceDBContext customerInvoiceDBContext) {
            _dbContext = customerInvoiceDBContext;
        }

        [HttpGet]
        public IActionResult Customers(string group = "A-E")
        {
            IEnumerable<Customer> customers;

            switch (group)
            {
                case "A-E":
                    customers = _dbContext.Customers.Where(c => !c.IsDeleted &&
						c.Name.StartsWith("A") || c.Name.StartsWith("B") || c.Name.StartsWith("C") ||
                        c.Name.StartsWith("D") || c.Name.StartsWith("E"));
                    break;

                case "F-K":
                    customers = _dbContext.Customers.Where(c => !c.IsDeleted &&
						c.Name.StartsWith("F") || c.Name.StartsWith("G") || c.Name.StartsWith("H") ||
                        c.Name.StartsWith("I") || c.Name.StartsWith("J") || c.Name.StartsWith("K"));
                    break;

                case "L-R":
                    customers = _dbContext.Customers.Where(c => !c.IsDeleted &&
						c.Name.StartsWith("L") || c.Name.StartsWith("M") || c.Name.StartsWith("N") ||
                        c.Name.StartsWith("O") || c.Name.StartsWith("P") || c.Name.StartsWith("Q") ||
                        c.Name.StartsWith("R"));
                    break;

                case "S-Z":
                    customers = _dbContext.Customers.Where(c => !c.IsDeleted &&
						c.Name.StartsWith("S") || c.Name.StartsWith("T") || c.Name.StartsWith("U") ||
                        c.Name.StartsWith("V") || c.Name.StartsWith("W") || c.Name.StartsWith("X") ||
                        c.Name.StartsWith("Y") || c.Name.StartsWith("Z"));
                    break;

                default:
                    customers = _dbContext.Customers;
                    break;
            }

            ViewBag.CurrentGroup = group;
            return View("Customers", customers.ToList());
        }


        [HttpGet("/Customers")]

        public IActionResult Add(string group)
        {
			ViewBag.CurrentGroup = group;
			return View("AddCustomer", new Customer());
        }



        [HttpGet("/Customers{id}/edit")]
        public IActionResult Edit(int id,string group)
        {
            var cust= _dbContext.Customers.Find(id);
			ViewBag.CurrentGroup = group;
			return View("Edit",cust);
        }



        [HttpPost("/Customers/Save")]

        public IActionResult SaveCustomer(Customer customer,string group)
        {
            if (ModelState.IsValid)
            {
                string actionMessage;
                if (customer.CustomerId == 0)
                {
					_dbContext.Customers.Add(customer);
                    actionMessage = "Customer added successfully!";

                }
                else
                {
					_dbContext.Customers.Update(customer);
                    actionMessage = "Customer updated successfully!";
                }
				_dbContext.SaveChanges();
				ViewBag.CurrentGroup = group;
                TempData["LastActionMessage"] = actionMessage;
                return RedirectToAction("Customers", "Customers", new { group });
            }
			ViewBag.CurrentGroup = group;
			return View(customer.CustomerId == 0 ? "AddCustomer" : "Edit", customer);
        }

        [HttpGet("/Customers{id}/Delete{group}")]
        public IActionResult Delete(int id,string group) 
        {
            var customer = _dbContext.Customers.Find(id);
            if(customer != null)
            {
                customer.IsDeleted = true;
                _dbContext.SaveChanges();
                TempData["LastActionMessage"] = "Customer deleted Successfully";
                TempData["LastDeletedCustomerId"] = id;
                TempData["LastDeletedCustomerName"] = customer.Name;
                TempData["Group"] = group;
			}
            return RedirectToAction("Customers", new {group});
        }

        [HttpGet]
        public IActionResult UndoDelete(int id)
		{
			if (id != null)
			{
				var customer = _dbContext.Customers.Find(id);
				if (customer != null)
				{
					customer.IsDeleted = false;
					_dbContext.SaveChanges();
                    TempData["LastActionMessage"] = "Course retrived Successfully";
                }

				string group = TempData["Group"]?.ToString();
				return RedirectToAction("Customers", new { group });
			}

			return RedirectToAction("Customers");
		}


        [HttpGet("/Customers{id}/invoice{invoiceId}")]

        public IActionResult Invoices(int id,int? invoiceId,string group) 
        {
            var customer = _dbContext.Customers
                .Include(c => c.Invoices)
                    .ThenInclude(i => i.InvoiceLineItems)
                .Include(c => c.Invoices)
                    .ThenInclude(i => i.PaymentTerms)
                .Select(c => new
                {
                    Customer = c,
                    Invoices = c.Invoices.Select(i => new Invoice
                    {
                        InvoiceId = i.InvoiceId,
                        InvoiceDate = i.InvoiceDate,
                        PaymentTotal = i.InvoiceLineItems.Sum(li => li.Amount ?? 0), 
                        PaymentTerms = i.PaymentTerms,
                        InvoiceLineItems = i.InvoiceLineItems
                    }).ToList()
                })
                .AsEnumerable()
                .Select(c =>
                {
                    c.Customer.Invoices = c.Invoices;
                    return c.Customer;
                })
                .FirstOrDefault(c => c.CustomerId == id && !c.IsDeleted);
            var paymentTerms = _dbContext.PaymentTerms.ToList();
            if (customer == null)
            {
                return NotFound();
            }

            var selectedInvoice = customer.Invoices.FirstOrDefault(i => i.InvoiceId == (invoiceId == 0 ? customer.Invoices.FirstOrDefault()?.InvoiceId : invoiceId));

            var model = new CustomerInvoicesModel
            {
                Customer = customer,
                SelectedInvoice = selectedInvoice,
                PaymentTerms= paymentTerms,
                LineItems = selectedInvoice?.InvoiceLineItems ?? new List<InvoiceLineItem>()
            };
            ViewBag.CurrentGroup = group;
            return View(model);
        }

        [HttpPost]
        public IActionResult AddLineItem(int id, int invoiceId, string description, double amount)
        {
            var invoice = _dbContext.Invoices.Include(i => i.InvoiceLineItems).FirstOrDefault(i => i.InvoiceId == invoiceId);
            if (invoice != null)
            {
                invoice.InvoiceLineItems.Add(new InvoiceLineItem
                {
                    Description = description,
                    Amount = amount
                });
               
                _dbContext.SaveChanges();
                TempData["LastActionMessage"] = "Line Item added Successfully";
            }
             string url = $"/Customers{id}/invoice{invoiceId}";
            return Redirect(url);
        }


        [HttpPost]
        public IActionResult AddInvoice(int customerId, int paymentTermsId, DateTime invoiceDate,string group)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer != null)
            {
                var invoice = new Invoice
                {
                    InvoiceDate = invoiceDate,
                    PaymentTermsId = paymentTermsId,
                    CustomerId = customerId
                };
                _dbContext.Invoices.Add(invoice);
                _dbContext.SaveChanges();
                TempData["LastActionMessage"] = "Invoice added Successfully";
                string url = $"/Customers{customerId}/invoice{invoice.InvoiceId}?group={group}";
                return Redirect(url);

            }
            return RedirectToAction("Invoices", new { customerId });
        }


    }
}
