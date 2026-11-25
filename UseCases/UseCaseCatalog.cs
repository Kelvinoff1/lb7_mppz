using StoreSystem.Abstractions;

namespace StoreSystem.UseCases;

public class UseCaseCatalog
{
    public ViewProductsByCategoryUseCase ViewProductsByCategory { get; }   
    public ViewCustomerOrdersUseCase ViewCustomerOrders { get; }           
    public ViewCustomerProfileUseCase ViewCustomerProfile { get; }         
    public ViewProductSuppliersUseCase ViewProductSuppliers { get; }       
    public CreateOrderUseCase CreateOrder { get; }                         
    public AddProductToOrderUseCase AddProductToOrder { get; }             
    public PayOrderUseCase PayOrder { get; }                               
    public ApplyLoyaltyDiscountUseCase ApplyLoyaltyDiscount { get; }       
    public CheckProductStockUseCase CheckProductStock { get; }             
    public CompleteOrderUseCase CompleteOrder { get; }                     
    public CancelOrderUseCase CancelOrder { get; }                         
    public CheckoutOrderUseCase CheckoutOrder { get; }                     

    public UseCaseCatalog(IStoreRepository repository, IAuditLog log)
    {
        CheckProductStock = new CheckProductStockUseCase(repository);
        AddProductToOrder = new AddProductToOrderUseCase(repository, CheckProductStock, log);
        CreateOrder = new CreateOrderUseCase(repository, CheckProductStock, AddProductToOrder, log);
        ApplyLoyaltyDiscount = new ApplyLoyaltyDiscountUseCase();
        PayOrder = new PayOrderUseCase(repository, ApplyLoyaltyDiscount, log);
        ViewProductsByCategory = new ViewProductsByCategoryUseCase(repository);
        ViewCustomerOrders = new ViewCustomerOrdersUseCase(repository);
        ViewCustomerProfile = new ViewCustomerProfileUseCase(repository);
        ViewProductSuppliers = new ViewProductSuppliersUseCase(repository);
        CompleteOrder = new CompleteOrderUseCase(repository, log);
        CancelOrder = new CancelOrderUseCase(repository, log);
        CheckoutOrder = new CheckoutOrderUseCase(CreateOrder, PayOrder, CancelOrder, log);
    }
}
