/**
 *  Shopping cart examples with order, order line items, discounts, and 
 *  coupon with different calculations.
 */


//-------------------------------------------------------------------------------- 
//  DOMAIN CLASSES
//-------------------------------------------------------------------------------- 

public class Product
{
    public string SkuCode { get; set; }
    public string Category { get; set; }
    public string Supplier { get; set; }
    public decimal Price { get; set; }
}

public class Order
{
    public List<OrderLine> OrderLines { get; set; }
    public List<Discount> Discounts { get; set; }    

    public decimal Total
    {
        get { return OrderLines.Sum(x => x.SubTotal); }
    }

    public decimal AmountPayable
    {
        get 
        {
            var discounts = Discounts.Sum(x => x.Amount);
            return this.Total - discounts;
        }
    }
}

public class OrderLine
{
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public bool IsFree { get; set; }

    public decimal SubTotal
    {
        get { return (IsFree) ? 0 : (Product.Price * Quantity); }
    }
}

public class Discount
{
    public string Remarks { get; set; }
    public decimal Amount { get; set; }

    public Discount(string remarks, decimal amount)
    {
        this.Remarks = remark;
        this.Amount = amount;
    }
}

public class Coupon 
{
    public string Code { get; set; }
    public CouponType CouponType { get; set; }
    public string ItemCode { get; set; }
    public decimal Amount { get; set; }
}

public enum CouponType
{
    WholeOrderFixed,
    WholeOrderPercentage,
    SpecificProduct,
    SpecificSupplier,
    SpecificCategory,
    FreeGift
}


//-------------------------------------------------------------------------------- 
//  COUPON CALCULATION
//-------------------------------------------------------------------------------- 

public class CouponCalculatorFactory
{
    CouponType _type;
    IRepository _repository;

    public CouponCalculatorFactory(IRepository repository, CouponType type)
    {
        _repository = repository;
        _type = type;
    }

    public ICouponCalculator GetCalculator()
    {
        switch (_type)
        {
            case WholeOrderFixed      : return new WholeOrderFixedCalculator();
            case WholeOrderPercentage : return new WholeOrderPercentageCalculator();
            case SpecificProduct      : return new SpecificProductCalculator();
            case SpecificSupplier     : return new SpecificSupplierCalculator();
            case SpecificCategory     : return new SpecificCategoryCalculator();
            case FreeGift             : return new FreeGiftCalculator(repository);

            case default: throw new InvalidCouponTypeException();
        }
    }
}

public class InvalidCouponTypeException : Exception
{    
}

interface ICouponCalculator
{
    void Apply(Coupon coupon, Order order);
}

public class WholeOrderFixedCalculator : ICouponCalculator
{
    public void Apply(Coupon coupon, Order order)
    {
        order.Discounts.Add(new Discount("Discount for whole order: ", 
                                          coupon.Amount));
    }
}

public class WholeOrderPercentageCalculator : ICouponCalculator
{
    public void Apply(Coupon coupon, Order order)
    {
        var percentage = order.Total * coupon.Amount;
        order.Discounts.Add(new Discount("Discount for whole order (" + coupon.Amount + "%): ", 
                                          percentage));
    }
}

public class SpecificProductCalculator : ICouponCalculator
{
    public void Apply(Coupon coupon, Order order)
    {
        var discountedOrderLines = order.OrderLines.Where(x => x.Product.SkuCode == coupon.ItemCode);
        var count = discountedOrderLines.Count();
        if (count > 0)
        {
            var discount = coupon.Amount * count;
            order.Discounts.Add(new Discount("Discount for product + " + coupon.ItemCode + " x " + count + ": ", 
                                              discount));
        }
    }
}

public class SpecificSupplierCalculator : ICouponCalculator
{
    // Lazy to do implementation, same like SpecificProductCalculator
}

public class SpecificCategoryCalculator : ICouponCalculator
{
    // Lazy to do implementation, same like SpecificProductCalculator
}

public class FreeGiftCalculator: ICouponCalculator
{
    IRepository _repository;

    public FreeGiftCalculator(IRepository repository)
    {
        _repository = repository;
    }

    public void Apply(Order order, Coupon coupon)
    {
        var product = _repository.FindProductBySku(coupon.ItemCode);
        
        var orderLine = new OrderLine() {
            Product = product,
            Quantity = 1,
            IsFree = true
        };

        order.OrderLines.Add(orderLine);
    }
}


//-------------------------------------------------------------------------------- 
//  CLIENT CODE (Maybe in controller action or something)
//-------------------------------------------------------------------------------- 

/* Repository is a way to get things from database
   Implementation not shown as always :) */
IRepository repository = new SqlRepository();

/* Build order by adding product-quantity as order line, etc */
var order = new Order();
order.OrderLines.Add(/* bla bla bla */);

var couponCode = /* get coupon code submitted from user */;
var coupon = repository.GetCouponByCode(couponCode);

var factory = new CouponCalculatorFactory(repository, coupon.CouponType);

ICouponCalculator couponCalculator = factory.GetCalculator();

/* Apply the coupon to the order */
couponCalculator.Apply(coupon, order);

var amountYouNeedToPay = order.AmountPayable();

/* Enjoy your new swag. */