using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.DTOs
{
    public class OfferCreatedDTO
    {
        public int OfferId { get; set; }
        public int CarId { get; set; }
        public int BuyerId { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public bool IsCurrent { get; set; }
    }
}